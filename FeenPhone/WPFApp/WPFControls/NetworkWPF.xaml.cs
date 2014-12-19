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

namespace FeenPhone.WPFApp.Controls
{
    /// <summary>
    /// Interaction logic for NetworkWPF.xaml
    /// </summary>
    public partial class NetworkWPF : UserControl
    {

        System.Timers.Timer UIUpdateTimer;
        public NetworkWPF()
        {
            InitializeComponent();
            DataContext = this;

            LoadSettings();
            Settings.SaveSettings += Settings_SaveSettings;

            RemoteClient.OnDisconnected += RemoteClient_OnDisconnected;

            EventSource.OnLoginStatus += EventSource_OnLoginStatus;
            EventSource.OnPingReq += EventSource_OnPingReq;
            EventSource.OnPingResp += EventSource_OnPingResp;

            UIUpdateTimer = new System.Timers.Timer(5000);
            UIUpdateTimer.Start();
            UIUpdateTimer.Elapsed += UIUpdateTimer_Elapsed;
        }

        void UIUpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Client is RemoteClient)
                Client.SendPingReq();
        }
        
        void EventSource_OnPingResp(object sender, PingEventArgs e)
        {
            Console.WriteLine("Ping resp: {0}", e.Value);
        }

        public static DependencyProperty ControlsEnabledProperty = DependencyProperty.Register("ControlsEnabled", typeof(bool), typeof(NetworkWPF), new PropertyMetadata(true));
        public bool ControlsEnabled
        {
            get { return (bool)this.GetValue(ControlsEnabledProperty); }
            set { this.SetValue(ControlsEnabledProperty, value); }
        }

        private void RemoteClient_OnDisconnected(object sender, EventArgs e)
        {
            if (sender == Client)
                Dispatcher.BeginInvoke(new Action(OnDisconnected));
        }
        
        public void OnDisconnected()
        {
            ControlsEnabled = true;
            Disconnect();
        }

        private void LoadSettings()
        {
            var settings = Settings.Container;

            if (settings.Server != null)
                txtServer.Text = settings.Server;

            int port;
            if (!string.IsNullOrWhiteSpace(settings.Port) && int.TryParse(settings.Port, out port))
                txtPort.Text = settings.Port;
            else
                txtPort.Text = ServerHost.DefaultServerPort.ToString();

            if (settings.Nickname != null)
                txtNickname.Text = settings.Nickname;
        }

        private void Settings_SaveSettings(object sender, EventArgs e)
        {
            var settings = Settings.Container;
            settings.Server = txtServer.Text;
            settings.Port = txtPort.Text;
            settings.Nickname = txtNickname.Text;
        }

        int invalidLoginAttempts = 0;
        void EventSource_OnLoginStatus(object sender, BoolEventArgs e)
        {
            bool isLoggedIn = e.Value;
            if (!isLoggedIn)
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

        void EventSource_OnPingReq(object sender, PingEventArgs e)
        {
            Client.SendPingResp(e.Value);
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
                    int port;
                    if (!int.TryParse(target.txtPort.Text, out port))
                    {
                        port = ServerHost.DefaultServerPort;
                        target.txtPort.Text = port.ToString();
                    }

                    target.Disconnect();
                    Client = ServerHost.LocalClient = new LocalClient(target.User);
                    target.server = new FeenPhone.Server.ServerHost(TcpServerPort: port);
                }
                else
                {
                    Client = ServerHost.LocalClient = null;
                    if (target.server != null)
                        target.server.Dispose();
                    target.server = null;
                }
            }
        }

        private void Disconnect()
        {
            if (Client != null)
                Client.Dispose();
            btnConnect.Content = "Connect";
            ControlsEnabled = true;
            Client = null;
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
            if (Client != null)
            {
                Disconnect();
                return;
            }

            IPAddress IP;
            int port = 0;

            bool OK = true;

            if (IsServer == true)
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

            RemoteClient remClient;
            if (comboProt.Text == "UDP")
                remClient = new RemoteUDPClient(User, IP, port);
            else
                remClient = new RemoteTCPClient(User, IP, port);
            
            Client = remClient;
            invalidLoginAttempts = 0;
            EventSource.InvokeOnUserList(null, null);
            ControlsEnabled = false;
            btnConnect.Content = "Disconnect";
            remClient.Connect();
        }
    }
}
