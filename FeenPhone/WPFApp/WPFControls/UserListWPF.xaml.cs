using Alienseed.BaseNetworkServer.Accounting;
using FeenPhone.Client;
using FeenPhone.WPFApp.Models;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

namespace FeenPhone.WPFApp.Controls
{
    /// <summary>
    /// Interaction logic for UserListWPF.xaml
    /// </summary>
    public partial class UserListWPF : UserControl
    {
        private static DateTime nextAudioAlertPlay = DateTime.MinValue;

        public static DependencyProperty NotifyOnConnectProperty = DependencyProperty.Register("NotifyOnConnect", typeof(bool), typeof(UserListWPF), new PropertyMetadata(false));
        public bool NotifyOnConnect
        {
            get { return (bool)this.GetValue(NotifyOnConnectProperty); }
            set { this.SetValue(NotifyOnConnectProperty, value); }
        }

        public UserListWPF()
        {
            InitializeComponent();
            UsersList.ItemsSource = UserStatusRepo.Users;
            UserStatusRepo.Users.CollectionChanged += Users_CollectionChanged;

            LoadSettings();
            Settings.AppClosing += Settings_SaveSettings;

        }

        private void LoadSettings()
        {
            var settings = Settings.Container;

            NotifyOnConnect = settings.NotifyOnConnect;
        }

        private void Settings_SaveSettings(object sender, EventArgs e)
        {
            var settings = Settings.Container;

            settings.NotifyOnConnect = NotifyOnConnect;
        }

        public static void DisableAudioAlertForDuration(double seconds = 2)
        {
            nextAudioAlertPlay = DateTime.UtcNow.Add(TimeSpan.FromSeconds(seconds));
        }

        void Users_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && NotifyOnConnect)
            {
                Dispatcher.BeginInvoke(new Action(PlayNotificationSound));
            }
        }

        private void PlayNotificationSound()
        {
            if (DateTime.UtcNow >= nextAudioAlertPlay)
            {
                DisableAudioAlertForDuration();
                try
                {
                    System.Windows.Resources.StreamResourceInfo sri = Application.GetResourceStream(new Uri("WPFApp/Resources/audio/FeenPhoneDJAlert.wav", UriKind.Relative));

                    using (WaveStream ws =
                       new BlockAlignReductionStream(
                           WaveFormatConversionStream.CreatePcmStream(
                               new WaveFileReader(sri.Stream))))
                    {
                        var length = ws.Length;
                        if (length < int.MaxValue)
                        {
                            byte[] data = new byte[length];
                            var format = ws.WaveFormat;
                            int read = ws.Read(data, 0, (int)length);
                            EventSource.InvokePlaySoundEffect(this, format, data);
                        }
                    }
                }
                catch { }
            }
        }

    }
}
