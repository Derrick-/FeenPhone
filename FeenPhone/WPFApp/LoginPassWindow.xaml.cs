using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FeenPhone.WPFApp
{
    /// <summary>
    /// Interaction logic for LoginPassWindow.xaml
    /// </summary>
    public partial class LoginPassWindow : Window, IDisposable
    {
        /// <summary>
        /// Prompt for password entry in new modal window
        /// </summary>
        /// <param name="password">the initial password to show, and the resultant user response</param>
        /// <param name="message">Message to display to user in window</param>
        /// <param name="buttonText">Label for affirm/login button</param>
        /// <returns>False if canceled</returns>
        public static bool Prompt(ref string password, string message = null, string buttonText = "Login")
        {
            using (LoginPassWindow pwWindow = new LoginPassWindow(
                initialPassword: password,
                messsage: message,
                confirmButtonText: buttonText))
            {
                pwWindow.ShowDialog();
                if (!pwWindow.Canceled)
                {
                    password = pwWindow.GetInput();
                    return true;
                }
            }
            return false;
        }
        
        public static DependencyProperty ServerMessageProperty = DependencyProperty.Register("ServerMessage", typeof(string), typeof(LoginPassWindow), new PropertyMetadata(null));
        public static DependencyProperty ConfirmButtonTextProperty = DependencyProperty.Register("ConfirmButtonText", typeof(string), typeof(LoginPassWindow), new PropertyMetadata("Login"));

        public bool Canceled { get; private set; }

        public LoginPassWindow() : this(null) { }
        public LoginPassWindow(string initialPassword, string messsage = null, string confirmButtonText = "Login")
        {
            InitializeComponent();
            input.Text = initialPassword;
            SetValue(ServerMessageProperty, messsage);
            SetValue(ConfirmButtonTextProperty, confirmButtonText);
            Canceled = true;
            DataContext = this;
            Topmost = true;
        }

        public void Dispose()
        {
            input.Text = null;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            Canceled = false;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Canceled = true;
            Close();
        }

        internal string GetInput()
        {
            return input.Text.Trim();
        }
    }
}
