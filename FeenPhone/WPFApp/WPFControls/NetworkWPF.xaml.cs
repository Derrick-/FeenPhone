using FeenPhone.Client;
using FeenPhone.Server;
using FeenPhone.WPFApp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FeenPhone.WPFApp.Controls
{
    /// <summary>
    /// Interaction logic for NetworkWPF.xaml
    /// </summary>
    public partial class NetworkWPF : UserControl
    {

        public const int DefaultPort = 5150;

        const int reconnectAttempts = 10;
        static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(2.0);

        System.Timers.Timer UIUpdateTimer;

        bool DoServerAutostart = false;

        public NetworkWPF()
        {
            isInitializing = true;

            InitializeComponent();
            DataContext = this;

            LoadSettings();
            Settings.AppClosing += Settings_SaveSettings;

            RemoteClient.OnDisconnected += RemoteClient_OnDisconnected;

            EventSource.OnLoginStatus += EventSource_OnLoginStatus;
            EventSource.OnPingReq += EventSource_OnPingReq;
            EventSource.OnPingResp += EventSource_OnPingResp;

            UIUpdateTimer = new System.Timers.Timer(1000);
            UIUpdateTimer.Start();
            UIUpdateTimer.Elapsed += UIUpdateTimer_Elapsed;

            ProcessCommandLine();

            isInitializing = false;
        }

        private void ProcessCommandLine()
        {
            string pass = CommandArgs.GetArgValue("pass");
            if (pass != null)
            {
                SetServerPassword(pass);
                RequireAuth = true;
            }

            string strTcpPort = CommandArgs.GetArgValue("ServerTcpPort");
            int tcpPort;
            if (strTcpPort != null && int.TryParse(strTcpPort, out tcpPort))
                TCPPort = tcpPort;

            string strUdpPort = CommandArgs.GetArgValue("ServerUdpPort");
            int udpPort;
            if (strUdpPort != null && int.TryParse(strUdpPort, out udpPort))
                UDPPort = udpPort;

            if (CommandArgs.HasArg("StartTcpServer"))
            {
                TCPEnabled = true;
                DoServerAutostart = true;
            }

            if (CommandArgs.HasArg("StartUdpServer"))
            {
                UDPEnabled = true;
                DoServerAutostart = true;
            }
        }

        void UIUpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (DoServerAutostart)
            {
                DoServerAutostart = false;
                Dispatcher.Invoke(() =>
                {
                    IsServer = true;
                    tcTabs.SelectedIndex = 1;
                });
            }

            if (Client is RemoteClient)
                Client.SendPingReq();
        }

        void EventSource_OnPingResp(object sender, PingEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                txtPing.Text = string.Format("{0}ms", e.Value);
            }));
        }

        public static DependencyProperty ShowAdvancedControlsProperty = DependencyProperty.Register("ShowAdvancedControls", typeof(bool), typeof(NetworkWPF), new PropertyMetadata(true, OnAdvancedControlsChanged));
        private static void OnAdvancedControlsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NetworkWPF target = (NetworkWPF)d;
            if (((bool)e.NewValue) == false)
            {
                target.TCPEnabled = true;
                target.UDPEnabled = false;
                target.TelnetEnabled = false;
                target.comboProt.SelectedIndex = 0;
            }
        }

        public static DependencyProperty RequireAuthProperty = DependencyProperty.Register("RequireAuth", typeof(bool), typeof(NetworkWPF), new PropertyMetadata(false, OnRequireAuthChanged));
        public bool RequireAuth
        {
            get { return (bool)this.GetValue(RequireAuthProperty); }
            set { this.SetValue(RequireAuthProperty, value); }
        }
        private readonly bool isInitializing = false;
        private static void OnRequireAuthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            bool newValue = (bool)e.NewValue;
            var target = (NetworkWPF)d;
            if (!target.isInitializing && (bool)e.OldValue != newValue)
            {
                if (newValue)
                    target.Dispatcher.BeginInvoke(new Action(() => { target.PromptSetServerPassword(); }));
                else
                    target.ClearServerPassword();
            }
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

            if (!requestedDisconnect)
                BeginReconnect();
            else
                btnConnect.Content = "Connect";

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

            TCPEnabled = settings.TCPServer;
            UDPEnabled = settings.UDPServer;
            TelnetEnabled = settings.TelnetServer;

            TCPPort = settings.TCPPort;
            UDPPort = settings.UDPPort;
            TelnetPort = settings.TelnetPort;

            if (!string.IsNullOrWhiteSpace(settings.RequireServerPass))
            {
                FeenPhone.Accounting.PasswordOnlyRepo.RequirePassword = settings.RequireServerPass;
                RequireAuth = true;
            }
        }

        private void Settings_SaveSettings(object sender, EventArgs e)
        {
            var settings = Settings.Container;
            settings.Server = txtServer.Text;
            settings.Port = txtPort.Text;
            settings.Nickname = txtNickname.Text;

            settings.TCPServer = TCPEnabled;
            settings.UDPServer = UDPEnabled;
            settings.TelnetServer = TelnetEnabled;

            settings.TCPPort = TCPPort;
            settings.UDPPort = UDPPort;
            settings.TelnetPort = TelnetPort;

            if (RequireAuth)
                settings.RequireServerPass = FeenPhone.Accounting.PasswordOnlyRepo.RequirePassword;
            else
                settings.RequireServerPass = null;
        }

        void EventSource_OnLoginStatus(object sender, LoginStatusEventArgs e)
        {
            requestedDisconnect = false;
            Dispatcher.Invoke(new Action<LoginStatusEventArgs>((args) => { 
                InvokeLoginEvent(args);
                btnConnect.Content = "Disconnect"; 
            }), e);
        }

        int invalidLoginAttempts = 0;
        private string Password = null;
        private void InvokeLoginEvent(LoginStatusEventArgs e)
        {
            UserListWPF.DisableAudioAlertForDuration(5.0);

            bool isLoggedIn = e.isLoggedIn;
            int version = e.version;
            string message = e.message;

            if (message != null)
                Console.WriteLine(message);

            var client = Client;

            if (!isLoggedIn && client != null && client.IsConnected)
            {
                Console.WriteLine("Server requests login.");
                if (invalidLoginAttempts == 0)
                {
                    invalidLoginAttempts++;
                    client.SendLoginInfo();
                }
                else
                {
                    invalidLoginAttempts++;
                    if (version >= 1)
                    {
                        var pass = client.Password;
                        if (LoginPassWindow.Prompt(password: ref pass, message: message) && !string.IsNullOrWhiteSpace(pass))
                        {
                            Password = client.Password = pass;
                            client.SendLoginInfo();
                            invalidLoginAttempts = 0;
                        }
                        else
                        {
                            Console.WriteLine("Server login canceled.");
                            client.Dispose();
                            if (Client == client)
                                Client = null;
                        }

                    }
                    else
                    {
                        Console.WriteLine("Server login rejected.");
                        client.Dispose();
                        if (Client == client)
                            Client = null;
                    }
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
            if (Client is RemoteClient)
                Client.SendPingResp(e.Value);
        }

        public static DependencyProperty IsServerProperty = DependencyProperty.Register("IsServer", typeof(bool?), typeof(NetworkWPF), new PropertyMetadata(false, OnIsServerChanged));
        public static DependencyProperty TCPEnabledProperty = DependencyProperty.Register("TCPEnabled", typeof(bool?), typeof(NetworkWPF), new PropertyMetadata(true, OnTCPEnabledChanged));
        public static DependencyProperty UDPEnabledProperty = DependencyProperty.Register("UDPEnabled", typeof(bool?), typeof(NetworkWPF), new PropertyMetadata(false, OnUDPEnabledChanged));
        public static DependencyProperty TelnetEnabledProperty = DependencyProperty.Register("TelnetEnabled", typeof(bool?), typeof(NetworkWPF), new PropertyMetadata(false, OnTelnetEnabledChanged));

        public static DependencyProperty TCPPortEnabledProperty = DependencyProperty.Register("TCPPortEnabled", typeof(bool?), typeof(NetworkWPF), new PropertyMetadata(true));
        public static DependencyProperty UDPPortEnabledProperty = DependencyProperty.Register("UDPPortEnabled", typeof(bool?), typeof(NetworkWPF), new PropertyMetadata(true));
        public static DependencyProperty TelnetPortEnabledProperty = DependencyProperty.Register("TelnetPortEnabled", typeof(bool?), typeof(NetworkWPF), new PropertyMetadata(true));

        public static DependencyProperty TCPPortProperty = DependencyProperty.Register("TCPPort", typeof(int), typeof(NetworkWPF), new PropertyMetadata(5150, OnTCPPortChanged));
        public static DependencyProperty UDPPortProperty = DependencyProperty.Register("UDPPort", typeof(int), typeof(NetworkWPF), new PropertyMetadata(5150, OnUDPPortChanged));
        public static DependencyProperty TelnetPortProperty = DependencyProperty.Register("TelnetPort", typeof(int), typeof(NetworkWPF), new PropertyMetadata(23, OnTelnetPortChanged));
        public static DependencyProperty TCPPortTextProperty = DependencyProperty.Register("TCPPortText", typeof(string), typeof(NetworkWPF), new PropertyMetadata("5150", OnTCPPortTextChanged));
        public static DependencyProperty UDPPortTextProperty = DependencyProperty.Register("UDPPortText", typeof(string), typeof(NetworkWPF), new PropertyMetadata("5150", OnUDPPortTextChanged));
        public static DependencyProperty TelnetPortTextProperty = DependencyProperty.Register("TelnetPortText", typeof(string), typeof(NetworkWPF), new PropertyMetadata("23", OnTelnetPortTextChanged));

        public bool IsServer
        {
            get { return (bool?)this.GetValue(IsServerProperty) == true; }
            set { this.SetValue(IsServerProperty, value); }
        }
        public bool TCPEnabled
        {
            get { return (bool?)this.GetValue(TCPEnabledProperty) == true; }
            set { this.SetValue(TCPEnabledProperty, value); }
        }
        public bool UDPEnabled
        {
            get { return (bool?)this.GetValue(UDPEnabledProperty) == true; }
            set { this.SetValue(UDPEnabledProperty, value); }
        }
        public bool TelnetEnabled
        {
            get { return (bool?)this.GetValue(TelnetEnabledProperty) == true; }
            set { this.SetValue(TelnetEnabledProperty, value); }
        }

        public int TCPPort
        {
            get { return (int)this.GetValue(TCPPortProperty); }
            set { this.SetValue(TCPPortProperty, value); }
        }
        public int UDPPort
        {
            get { return (int)this.GetValue(UDPPortProperty); }
            set { this.SetValue(UDPPortProperty, value); }
        }
        public int TelnetPort
        {
            get { return (int)this.GetValue(TelnetPortProperty); }
            set { this.SetValue(TelnetPortProperty, value); }
        }

        private static void OnTCPPortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NetworkWPF target = d as NetworkWPF;
            if (target != null)
            {
                if (target.server != null) target.server.TCPServerPort = (int)e.NewValue;
                target.SetValue(TCPPortTextProperty, e.NewValue.ToString());
            }
        }

        private static void OnUDPPortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NetworkWPF target = d as NetworkWPF;
            if (target != null)
            {
                if (target.server != null) target.server.UDPServerPort = (int)e.NewValue;
                target.SetValue(UDPPortTextProperty, e.NewValue.ToString());
            }
        }

        private static void OnTelnetPortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NetworkWPF target = d as NetworkWPF;
            if (target != null)
            {
                if (target.server != null) target.server.TelnetServerPort = (int)e.NewValue;
                target.SetValue(TelnetPortTextProperty, e.NewValue.ToString());
            }
        }

        private static void OnTCPPortTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((NetworkWPF)d).TryUpdatePortFromString(TCPPortProperty, e, cannotBe: ((NetworkWPF)d).TelnetPort);
        }
        private static void OnUDPPortTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((NetworkWPF)d).TryUpdatePortFromString(UDPPortProperty, e);
        }
        private static void OnTelnetPortTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((NetworkWPF)d).TryUpdatePortFromString(TelnetPortProperty, e, cannotBe: ((NetworkWPF)d).TCPPort);
        }
        private void TryUpdatePortFromString(DependencyProperty p, DependencyPropertyChangedEventArgs e, int? cannotBe = null)
        {
            int newValue;
            if (int.TryParse((string)(e.NewValue), out newValue) &&
                newValue > 0 &&
                newValue <= UInt16.MaxValue &&
                (!cannotBe.HasValue || cannotBe.Value != newValue))
            {
                SetValue(p, newValue);
            }
            else
                SetValue(e.Property, e.OldValue);
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
                    if (target.TCPEnabled || target.UDPEnabled || target.TelnetEnabled)
                    {
                        target.DisconnectRequested();
                        Client = ServerHost.LocalClient = new LocalClient(target.User);
                        target.server = new FeenPhone.Server.ServerHost();
                        target.server.TCPServerPort = target.TCPPort;
                        target.server.UDPServerPort = target.UDPPort;
                        target.server.TelnetServerPort = target.TelnetPort;
                        target.server.InitServers(target.TCPEnabled, target.UDPEnabled, target.TelnetEnabled);
                        if (!target.server.AnyServersAreRunning())
                        {
                            Console.WriteLine("No servers were able to be started.");
                            target.SetValue(IsServerProperty, false);
                        }
                    }
                    else
                    {
                        Console.WriteLine("No servers enabled to run.");
                        target.SetValue(IsServerProperty, false);
                    }
                }
                else
                {
                    Client = ServerHost.LocalClient = null;
                    if (target.server != null)
                        target.server.Dispose();
                    target.server = null;
                }
                target.UpdatePortEnabledBindings();
            }
        }

        private void UpdatePortEnabledBindings()
        {
            SetValue(UDPPortEnabledProperty, !IsServer || !UDPEnabled);
            SetValue(TCPPortEnabledProperty, !IsServer || !TCPEnabled);
            SetValue(TelnetPortEnabledProperty, !IsServer || !TelnetEnabled);
        }


        private static void OnTCPEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NetworkWPF target = d as NetworkWPF;
            if (target != null)
            {
                if (target.server != null)
                {
                    target.server.EnableTCP((bool)e.NewValue);
                }
                target.UpdatePortEnabledBindings();
            }
        }

        private static void OnUDPEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NetworkWPF target = d as NetworkWPF;
            if (target != null)
            {
                if (target.server != null)
                {
                    target.server.EnableUDP((bool)e.NewValue);
                }
                target.UpdatePortEnabledBindings();
            }
        }

        private static void OnTelnetEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NetworkWPF target = d as NetworkWPF;
            if (target != null)
            {
                if (target.server != null)
                {
                    target.server.EnableTelnet((bool)e.NewValue);
                }
                target.UpdatePortEnabledBindings();
            }
        }

        private void DisconnectRequested()
        {
            requestedDisconnect = true;
            btnConnect.Content = "Connect";
            Disconnect();
        }

        private void Disconnect()
        {
            if (Client != null)
                Client.Dispose();
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

        bool requestedDisconnect = true;
        int reconnectsRemaining = reconnectAttempts;
        DispatcherTimer reconTimer = null;
        private void BeginReconnect()
        {
            if (reconTimer == null)
                reconTimer = new DispatcherTimer(ReconnectDelay, DispatcherPriority.ContextIdle, new EventHandler(DoReconnect), this.Dispatcher);

            if (!reconTimer.IsEnabled)
            {
                reconnectsRemaining = reconnectAttempts;
                reconTimer.Start();
            }
        }

        private void DoReconnect(object sender, EventArgs e)
        {
            reconnectsRemaining--;
            if (reconnectsRemaining <= 0 || requestedDisconnect || Client != null || !TryConnect())
            {
                if (Client == null)
                    btnConnect.Content = "Connect";

                requestedDisconnect = true;
                reconTimer.Stop();
            }
            else if (Client == null || !Client.IsConnected)
            {
                btnConnect.Content = string.Format("Reconnecting ({0}/{1})", reconnectAttempts - reconnectsRemaining, reconnectAttempts);
                requestedDisconnect = false;
            }
        }


        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (Client != null)
                DisconnectRequested();
            else
                TryConnect();
        }

        private bool TryConnect()
        {
            if (Client != null)
            {
                if (Client.IsConnected)
                    return false;
                else
                {
                    Client.Dispose();
                    Client = null;
                }
            }

            IPAddress IP;
            int port = 0;

            bool OK = true;

            if (IsServer)
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

            if (!OK) return false;

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
                    return false;
                }

                //IP = ips.Where(m => m.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6).FirstOrDefault();
                //if (IP == null)
                {
                    IP = ips.Where(m => m.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault();
                }
                if (IP == null)
                {
                    Console.WriteLine("No valid addresses for {0}", servername);
                    return false;
                }
            }

            RemoteClient remClient;
            if (comboProt.Text == "UDP")
                remClient = new RemoteUDPClient(User, IP, port);
            else
                remClient = new RemoteTCPClient(User, IP, port);

            Client = remClient;

            invalidLoginAttempts = 0;
            Client.Password = Password;

            EventSource.InvokeOnUserList(null, null);
            ControlsEnabled = false;
            remClient.Connect();
            btnConnect.Content = "Disconnect";

            return true;
        }

        private void ManageAuth_Click(object sender, RoutedEventArgs e)
        {
            PromptSetServerPassword();
        }

        private void PromptSetServerPassword()
        {
            var password = FeenPhone.Accounting.PasswordOnlyRepo.RequirePassword;
            if (LoginPassWindow.Prompt(ref password, "Set Server Password.", "Set Pass"))
                SetServerPassword(password);
            else
            {
                if (password == null)
                    RequireAuth = false;
                else
                    Console.WriteLine("Server password unchanged.");
            }
        }

        private void SetServerPassword(string newPass)
        {
            if (string.IsNullOrWhiteSpace(newPass))
            {
                ClearServerPassword();
            }
            else
            {
                FeenPhone.Accounting.PasswordOnlyRepo.RequirePassword = newPass;
                Console.WriteLine("Server password updated.");
            }
        }

        private void ClearServerPassword()
        {
            RequireAuth = false;
            FeenPhone.Accounting.PasswordOnlyRepo.RequirePassword = null;
            Console.WriteLine("Server password cleared.");
        }

        private void AddressBook_Button_Click(object sender, RoutedEventArgs e)
        {
            var addrWindow = new AddressBookWindow(txtServer.Text, txtPort.Text);
            addrWindow.ShowDialog();
            var selected = addrWindow.Selected;
            if (selected != null && selected.Host != null)
            {
                txtServer.Text = selected.Host.ToString();
                txtPort.Text = selected.Port.ToString();
                if (!string.IsNullOrWhiteSpace(selected.Password))
                    Password = selected.Password;
            }
        }
    }
}
