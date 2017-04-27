using System;
using System.Windows;
using SetupApplication.Code;

namespace SetupApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            if (SetupProcess.IsSetupTracesExists())
            {
                Modify.Visibility = Visibility.Visible;
                Remove.Visibility = Visibility.Visible;
                Install.Visibility = Visibility.Hidden;
            }
        }

        private void BeginSetup()
        {
            if (SetupProcess.Setup(UserNameTxt.Text, PasswordTxt.Password, (bool)DisableAutoLogoffControl.IsChecked))
            {
                MessageBox.Show("Application successfully installed.");
            }
            else
            {
                MessageBox.Show("Oops!! Something went wrong. Installation Aborted.");
            }
        }

        private void Install_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BeginSetup();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                Close();
            }
        }

        private void Modify_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetupProcess.RollBack();
                BeginSetup();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                Close();
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SetupProcess.RollBack())
                {
                    MessageBox.Show("Application successfully removed.");
                }
                else
                {
                    MessageBox.Show("Failed to remove application.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                Close();
            }
        }
    }
}
