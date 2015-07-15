using FeenPhone.WPFApp.Controls;
using FeenPhone.WPFApp.Models;
using FeenPhone.WPFApp.Utils;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FeenPhone.WPFApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            if (Properties.Settings.Default.NeedsUpgrade)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.NeedsUpgrade = false;
            }

            InitializeComponent();

            LoadSettings();
            this.Closed += new EventHandler(Window_Closed);
            Settings.AppClosing += Settings_SaveSettings;

            FeenPhone.Client.EventSource.OnChat += EventSource_OnChat;
            UserStatusRepo.Users.CollectionChanged += Users_CollectionChanged;

            DataContext = this;
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            GlassHelper.ExtendGlassFrame(this, new Thickness(-1));
        }

        private void LoadSettings()
        {
            Properties.Settings settings = Settings.Container;

            SetValue(ShowAdvancedControlsProperty, CommandArgs.HasArg("StartUdpServer") || settings.ShowAdvancedControls);
        }

        private void Settings_SaveSettings(object sender, EventArgs e)
        {
            Properties.Settings settings = Settings.Container;

            settings.ShowAdvancedControls = (bool)GetValue(ShowAdvancedControlsProperty);
        }

        void Window_Closed(object sender, EventArgs e)
        {
            Settings.InvokeAppClosing(this);
            Settings.Container.Save();

            AudioIn.Dispose();
            AudioOut.Dispose();
        }

        private void EventSource_OnChat(object sender, Client.OnChatEventArgs e)
        {
            InvokeFlash();
        }

        void Users_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                InvokeFlash();
            }
        }

        private void InvokeFlash()
        {
            Dispatcher.BeginInvoke(new Action(() => { this.Flash(); }));
        }

        public static DependencyProperty ShowAboutBoxProperty = DependencyProperty.Register("ShowAboutBox", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));
        public bool ShowAboutBox
        {
            get { return (bool)GetValue(ShowAboutBoxProperty); }
            set { SetValue(ShowAboutBoxProperty, value); }
        }

        public static DependencyProperty ShowAdvancedControlsProperty = DependencyProperty.Register("ShowAdvancedControls", typeof(bool), typeof(MainWindow), new PropertyMetadata(true, OnAdvancedControlsChanged));
        private static void OnAdvancedControlsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool newValue = (bool)e.NewValue;
            var target = (MainWindow)d;
            target.Dispatcher.BeginInvoke(new Action(() =>
                {
                    target.SetAdvanced(newValue);
                }));
        }

        private void SetAdvanced(bool showAdvanced)
        {
            Network.SetValue(NetworkWPF.ShowAdvancedControlsProperty, showAdvanced);
            AudioIn.SetValue(AudioInWPF.ShowAdvancedControlsProperty, showAdvanced);
            AudioOut.SetValue(AudioOutWPF.ShowAdvancedControlsProperty, showAdvanced);
        }

        /// <summary>
        /// TitleBar_MouseDown - Drag if single-click, resize if double-click
        /// </summary>
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                if (e.ClickCount == 2)
                {
                    AdjustWindowSize();
                }
                else
                {
                    Application.Current.MainWindow.DragMove();
                }
        }

        /// <summary>
        /// Adjusts the WindowSize to correct parameters when Maximize button is clicked
        /// </summary>
        private void AdjustWindowSize()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }

        }

        private void Button_Help_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Command-line arguments:\n" +
                "\t-StartTcpServer\tAuto-start TCP server\n" +
                "\t-StartUdpServer\tAuto-start UDP server\n" +
                "\t-ServerTcpPort=[port]\tSet TCP server port\n" +
                "\t-ServerUdpPort=[port]\tSet UDP server port\n" +
                "\t-pass=[password]\tSet server password"+
                "\n\n"+
                "Example:\n\tFeenPhone.exe -servertcpport=5150 -starttcpserver -pass=worms");
        }
    }
}
