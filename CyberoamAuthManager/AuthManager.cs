using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using Windows.UI.Notifications;

namespace CyberoamAuthManager
{
    /// <summary>
    /// Class to manage cyberoam authentication.
    /// </summary>
    static class AuthManager
    {
        /// <summary>
        /// Login url.
        /// </summary>
        public static string LoginUrl { get; set; } = @"http://10.0.0.1:8090/login.xml";

        /// <summary>
        /// Logout url.
        /// </summary>
        public static string LogoutUrl { get; set; } = @"http://10.0.0.1:8090/logout.xml";

        /// <summary>
        /// Mode number for login operation.
        /// </summary>
        public static int LoginMode { get; set; } = 191;

        /// <summary>
        /// Mode number for logout operation.
        /// </summary>
        public static int LogoutMode { get; set; } = 193;

        /// <summary>
        /// Product Type:
        /// 0 - Desktop
        /// 1 - iPhone and iPad
        /// 2 - Android
        /// </summary>
        public static int ProductType { get; set; } = 0;

        /// <summary>
        /// Method to login into cyberoam.
        /// </summary>
        /// <param name="userName">Cyberoam user name.</param>
        /// <param name="password">Cyberoam password.</param>
        public static void Login(string userName, string password)
        {
            StringBuilder postData = new StringBuilder();
            postData.Append(String.Format("mode={0}&", LoginMode));
            postData.Append(String.Format("username={0}&", HttpUtility.UrlEncode(userName)));
            postData.Append(String.Format("password={0}&", HttpUtility.UrlEncode(password)));
            postData.Append(String.Format("a={0}&", DateTime.Now.TimeOfDay.Ticks + ProductType));
            postData.Append(String.Format("producttype={0}", ProductType));
            MakeRequest(postData.ToString(), LoginUrl);
        }

        /// <summary>
        /// Method to logout from cyberoam.
        /// </summary>
        /// <param name="userName">Cyberoam user name.</param>
        public static void Logout(string userName)
        {
            StringBuilder postData = new StringBuilder();
            postData.Append(String.Format("mode={0}&", LogoutMode));
            postData.Append(String.Format("username={0}&", HttpUtility.UrlEncode(userName)));
            postData.Append(String.Format("a={0}&", DateTime.Now.TimeOfDay.Ticks));
            postData.Append(String.Format("producttype={0}", ProductType));

            MakeRequest(postData.ToString(), LogoutUrl);
        }

        /// <summary>
        /// Method to make http post request.
        /// </summary>
        /// <param name="postData">Payload data.</param>
        /// <param name="url">Request url.</param>
        private static void MakeRequest(string postData, string url)
        {
            ASCIIEncoding ascii = new ASCIIEncoding();
            byte[] postBytes = ascii.GetBytes(postData);

            // set up request object
            HttpWebRequest request;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postBytes.Length;

                // add post data to request
                Stream postStream = request.GetRequestStream();
                postStream.Write(postBytes, 0, postBytes.Length);

                var responseString = new StreamReader(request.GetResponse().GetResponseStream()).ReadToEnd();

                ParseResponse(responseString);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Method to parse response string and display message.
        /// </summary>
        /// <param name="response">Response string.</param>
        private static void ParseResponse(string response)
        {
            string logo = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\Cyberoam Auth Manager\\info.ico");
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(response);
            var messageNode = xml.SelectSingleNode("/requestresponse/message");
            string toastXml = $@"<toast>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>Cyberoam Auth Manager</text>
                            <text>{messageNode.InnerText}</text>
                            <image src='{logo}' placement='appLogoOverride' hint-crop='circle'/>
                        </binding>
                    </visual>
                </toast>";
            Windows.Data.Xml.Dom.XmlDocument doc = new Windows.Data.Xml.Dom.XmlDocument();
            doc.LoadXml(toastXml);

            ToastNotification toast = new ToastNotification(doc);
            ToastNotificationManager.CreateToastNotifier("Cyberoam Auth Manager").Show(toast);
        }
    }
}
