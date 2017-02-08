using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
            if (SetupProcess.Setup(UserNameTxt.Text, PasswordTxt.Password))
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
                this.Close();
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
                this.Close();
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SetupProcess.RollBack()) {
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
                this.Close();
            }
        }
    }
}
