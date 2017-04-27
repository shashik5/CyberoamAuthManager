using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using CryptManager;
using Microsoft.Win32.TaskScheduler;
using System.Collections.Generic;

namespace SetupApplication.Code
{
    /// <summary>
    /// Class to store task details.
    /// </summary>
    class Task
    {
        /// <summary>
        /// Task name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Task description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Arguments to execute exe file.
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// List of state change type triggers to be created for this task.
        /// </summary>
        public List<TaskSessionStateChangeType> StateChangeTypes;

        /// <summary>
        /// Set true to create logon trigger for this task.
        /// </summary>
        public bool IsLogonTriggerRequired { get; set; } = false;
    }

    /// <summary>
    /// Class to perform installation process.
    /// </summary>
    static class SetupProcess
    {
        /// <summary>
        /// Variable to store current system user name.
        /// </summary>
        private static string SystemUserName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

        /// <summary>
        /// Login task name in task scheduler.
        /// </summary>
        private static string LoginTaskName = string.Concat("CyberoamLogin_", Regex.Replace(SystemUserName, @"\\", "_"));

        /// <summary>
        /// Logout task name in task scheduler.
        /// </summary>
        private static string LogoutTaskName = string.Concat("CyberoamLogout_", Regex.Replace(SystemUserName, @"\\", "_"));

        /// <summary>
        /// Flie paths.
        /// </summary>
        private static string DataXmlPath = @"Data\dat.xml",
            ExeFileName = "\\CyberoamAuthManager.exe",
            ExePath = string.Concat("Data", ExeFileName),
            TargetAppFolder = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\Cyberoam Auth Manager"),
            DataXmlFileName = "\\dat.xml";

        /// <summary>
        /// Method to store user data in xml file.
        /// </summary>
        /// <param name="userData">User data.</param>
        private static void UpdateUserDataInXml(string userData)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(File.ReadAllText(DataXmlPath));
            xml.SelectSingleNode("/CyberoamAuthManager/Auth").InnerText = Crypto.Encode(userData);
            xml.Save(DataXmlPath);
        }

        /// <summary>
        /// Method to copy required files in app directory.
        /// </summary>
        private static void ExportFiles()
        {
            if (!Directory.Exists(TargetAppFolder))
            {
                Directory.CreateDirectory(TargetAppFolder);
            }
            File.Copy(DataXmlPath, string.Concat(TargetAppFolder, DataXmlFileName), true);
            File.Copy(ExePath, string.Concat(TargetAppFolder, ExeFileName), true);
            File.Copy(@"Data\CryptManager.dll", string.Concat(TargetAppFolder, "\\CryptManager.dll"), true);
            File.Copy(@"Data\info.ico", string.Concat(TargetAppFolder, "\\info.ico"), true);
        }

        /// <summary>
        /// Method to create shortcut file in destination.
        /// </summary>
        /// <param name="source">Source file path.</param>
        /// <param name="destination">Destination file path.</param>
        private static void CreateShortcut(string source, string destination)
        {
            File.Delete(destination);
            var shell = new IWshRuntimeLibrary.WshShell();
            var shortcut = shell.CreateShortcut(destination) as IWshRuntimeLibrary.IWshShortcut;
            shortcut.TargetPath = source;
            shortcut.WorkingDirectory = source;
            shortcut.Description = "Cuberoam Auth Manager";
            shortcut.Save();
        }

        /// <summary>
        /// Method to rollback installation process.
        /// </summary>
        public static bool RollBack()
        {
            try
            {
                string shortcutFile = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "\\CyberoamAuthManager.lnk");

                if (File.Exists(shortcutFile))
                {
                    File.Delete(shortcutFile);
                }

                if (Directory.Exists(TargetAppFolder))
                {
                    Directory.Delete(TargetAppFolder, true);
                }

                using (TaskService ts = new TaskService())
                {
                    if (ts.RootFolder.Tasks.Exists(LoginTaskName))
                    {
                        ts.RootFolder.DeleteTask(LoginTaskName);
                    }

                    if (ts.RootFolder.Tasks.Exists(LogoutTaskName))
                    {
                        ts.RootFolder.DeleteTask(LogoutTaskName);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Method to add task in task scheduler.
        /// </summary>
        /// <param name="task">Taks to be scheduled.</param>
        private static void ScheduleTask(Task task)
        {
            using (TaskService ts = new TaskService())
            {
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = task.Description;
                foreach (TaskSessionStateChangeType stateChangeType in task.StateChangeTypes)
                {
                    td.Triggers.Add(new SessionStateChangeTrigger(stateChangeType) { UserId = SystemUserName, Enabled = true });
                }
                if (task.IsLogonTriggerRequired)
                {
                    td.Triggers.Add(new LogonTrigger() { UserId = SystemUserName, Enabled = true });
                }
                td.Actions.Add(new ExecAction(string.Concat(TargetAppFolder, ExeFileName), task.Arguments, null));
                ts.RootFolder.RegisterTaskDefinition(string.Concat(task.Name), td);
            }
        }

        /// <summary>
        /// Method to begin setup process.
        /// </summary>
        /// <param name="userName">Cyberoam user name.</param>
        /// <param name="password">Cyberoam password.</param>
        /// <param name="disableLogoff">Bool value to disable auto logoff.</param>
        /// <returns></returns>
        public static bool Setup(string userName, string password, bool disableLogoff)
        {
            try
            {
                StringBuilder userData = new StringBuilder();
                userData.Append(userName).Append(';').Append(password);

                UpdateUserDataInXml(userData.ToString());
                ExportFiles();
                //CreateShortcut(string.Concat(TargetAppFolder, ExeFileName), string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.Startup), ExeFileName.Replace(".exe", ".lnk")));
                ScheduleTask(new Task
                {
                    Name = LoginTaskName,
                    Description = "Login to cyberoam account.",
                    StateChangeTypes = new List<TaskSessionStateChangeType> {
                        TaskSessionStateChangeType.SessionUnlock,
                        TaskSessionStateChangeType.ConsoleConnect
                    },
                    IsLogonTriggerRequired = true,
                    Arguments = "login"
                });

                if (!disableLogoff)
                {
                    ScheduleTask(new Task
                    {
                        Name = LogoutTaskName,
                        Description = "Logout from cyberoam account.",
                        StateChangeTypes = new List<TaskSessionStateChangeType> {
                        TaskSessionStateChangeType.SessionLock,
                        TaskSessionStateChangeType.ConsoleDisconnect
                    },
                        Arguments = "logout"
                    }); 
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                RollBack();
                return false;
            }
        }

        /// <summary>
        /// Method to find any traces of previous installations.
        /// </summary>
        /// <returns>Returns true if setup traces exists.</returns>
        public static bool IsSetupTracesExists()
        {
            TaskService ts = new TaskService();
            if (File.Exists(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "\\CyberoamAuthManager.lnk")) || Directory.Exists(TargetAppFolder) || ts.RootFolder.Tasks.Exists(LoginTaskName) || ts.RootFolder.Tasks.Exists(LogoutTaskName))
            {
                return true;
            }

            return false;
        }
    }
}
