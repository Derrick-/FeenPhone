using FeenPhone.Client;
using FeenPhone.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace FeenPhone.WPFControls
{
    /// <summary>
    /// Interaction logic for NetworkWPF.xaml
    /// </summary>
    public partial class NetworkWPF : UserControl
    {

        public NetworkWPF()
        {
            InitializeComponent();
            DataContext = this;

            EventSource.OnLoginStatus += EventSource_OnLoginStatus;
        }

        int invalidLoginAttempts = 0;
        void EventSource_OnLoginStatus(object sender, BoolEventArgs e)
        {
            bool isLoggedIn = e.Value;
            if(!isLoggedIn)
            {
                if (invalidLoginAttempts == 0)
                {
                    invalidLoginAttempts++;
                    Console.WriteLine("Server requests login.");
                    Client.SendLoginInfo();
                }
                else
                {
                    Console.WriteLine("Server login rejected.");
                    Client.Dispose();
                    Client = null;
                }
            }
            else
            {
                Console.WriteLine("Server login accepted.");
                invalidLoginAttempts = 0;
            }
        }

        public static DependencyProperty IsServerProperty = DependencyProperty.Register("IsServer", typeof(bool?), typeof(NetworkWPF), new PropertyMetadata(false, OnIsServerChanged));

        public bool? IsServer
        {
            get { return (bool?)this.GetValue(IsServerProperty); }
            set { this.SetValue(IsServerProperty, value); }
        }

        internal static BaseClient Client { get; private set; }

        public class LocalUser : Alienseed.BaseNetworkServer.Accounting.IUserClient
        {
            public LocalUser(string nickname)
            {
                Nickname = nickname;
            }

            public Alienseed.BaseNetworkServer.Accounting.IClient Client
            {
                get { throw new NotImplementedException(); }
            }

            public bool SetClient(Alienseed.BaseNetworkServer.Accounting.IClient client)
            {
                throw new NotImplementedException();
            }

            public string Username
            {
                get { return Nickname; }
            }

            public string Nickname { get; set; }

            public bool IsAdmin
            {
                get { return true; }
            }

            public Guid ID
            {
                get { return Guid.Empty; }
            }

            public bool Equals(Alienseed.BaseNetworkServer.Accounting.IUser other)
            {
                return other is LocalUser;
            }
        }

        LocalUser _User = null;
        internal LocalUser User
        {
            get { return _User ?? (_User = new LocalUser(txtNickname.Text)); }
        }

        ServerHost server = null;
        private static void OnIsServerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NetworkWPF target = d as NetworkWPF;
            if (target != null)
            {
                EventSource.InvokeOnUserList(null, null);
                if ((bool?)e.NewValue == true && target.server == null)
                {
                    target.server = new FeenPhone.Server.ServerHost();
                    Client = ServerHost.LocalClient = new LocalClient(target.User);
                }
                else
                {
                    Client = ServerHost.LocalClient = null;
                    target.server.Dispose();
                    target.server = null;
                }
            }
        }

        private void txtNickname_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox box = sender as TextBox;
            if (box != null)
            {
                if (!string.IsNullOrWhiteSpace(box.Text))
                    User.Nickname = box.Text;
                else
                    box.Text = User.Nickname;
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if(Client!=null)
            {
                if (Client.IsConnected)
                {
                    Console.WriteLine("You are already connected.");
                    return;
                }
                else
                {
                    Client.Dispose();
                    Client = null;
                }
            }
            IPAddress IP;
            int port = 0;

            bool OK = true;

            if(IsServer==true)
            {
                Console.WriteLine("Cannot connect while running a server");
                OK = false;
            }

            if (string.IsNullOrWhiteSpace(txtServer.Text))
            {
                Console.WriteLine("Server is required");
                OK = false;
            }

            if (string.IsNullOrWhiteSpace(txtPort.Text))
            {
                Console.WriteLine("Port is required");
                OK = false;
            }
            else
            {
                if (!int.TryParse(txtPort.Text, out port))
                {
                    Console.WriteLine("Invalid Port: {0}", txtPort.Text);
                    OK = false;
                }
            }

            if (!OK) return;

            string servername = txtServer.Text.Trim();
            if (!IPAddress.TryParse(servername, out IP))
            {
                IPAddress[] ips = null;
                try
                {
                    ips = Dns.GetHostAddresses(servername);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not resolve {0}: ", servername, ex.Message);
                    return;
                }

                //IP = ips.Where(m => m.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6).FirstOrDefault();
                //if (IP == null)
                {
                    IP = ips.Where(m => m.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault();
                }
                if (IP == null)
                {
                    Console.WriteLine("No valid addresses for {0}", servername);
                    return;
                }
            }
            var remClient = new RemoteClient(User, IP, port);
            Client = remClient;
            invalidLoginAttempts = 0;
            EventSource.InvokeOnUserList(null, null);
            remClient.Connect();
        }
    }
}
