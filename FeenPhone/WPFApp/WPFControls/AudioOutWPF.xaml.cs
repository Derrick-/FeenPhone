using FeenPhone.Audio;
using FeenPhone.WPFApp.Models;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for AudioOutWPF.xaml
    /// </summary>
    public partial class AudioOutWPF : UserControl, IDisposable
    {
        static ObservableCollection<OutputDeviceModel> OutputList = new ObservableCollection<OutputDeviceModel>();

        static ObservableCollection<UserAudioPlayerWPF> AudioPlayers = new ObservableCollection<UserAudioPlayerWPF>();

        [ImportMany(typeof(Audio.Codecs.INetworkChatCodec))]
        public IEnumerable<Audio.Codecs.INetworkChatCodec> Codecs { get; set; }

        public static DependencyProperty OutputListProperty = DependencyProperty.Register("OutputList", typeof(ObservableCollection<OutputDeviceModel>), typeof(AudioOutWPF), new PropertyMetadata(OutputList));
        public static DependencyProperty SelectedOutputProperty = DependencyProperty.Register("SelectedOutput", typeof(OutputDeviceModel), typeof(AudioOutWPF), new PropertyMetadata(null, OnOutputDeviceChanged));
        public static DependencyProperty SelectedOutputIndexProperty = DependencyProperty.Register("SelectedOutputIndex", typeof(int?), typeof(AudioOutWPF), new PropertyMetadata(-1));

        public static DependencyProperty AudioPlayersProperty = DependencyProperty.Register("AudioPlayers", typeof(ObservableCollection<UserAudioPlayerWPF>), typeof(AudioOutWPF), new PropertyMetadata(AudioPlayers));

        public OutputDeviceModel SelectedOutput
        {
            get { return (OutputDeviceModel)this.GetValue(SelectedOutputProperty); }
            set { this.SetValue(SelectedOutputProperty, value); }
        }

        public int? SelectedOutputIndex
        {
            get { return (int?)this.GetValue(SelectedOutputIndexProperty); }
            set { this.SetValue(SelectedOutputIndexProperty, value); }
        }

        private static void OnOutputDeviceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = d as AudioOutWPF;
            if (d != null)
                target.StopAll();
        }

        private void StopAll()
        {
            foreach (var player in AudioPlayers)
                player.Stop();
        }

        System.Timers.Timer UIUpdateTimer;
        public AudioOutWPF()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
                new CompositionContainer(new AssemblyCatalog(Assembly.GetExecutingAssembly())).ComposeParts(this);

            InitializeComponent();
            DataContext = this;

            InitializeOutputDevices();

            LoadSettings();
            Settings.AppClosing += Settings_SaveSettings;

            FeenPhone.Client.EventSource.OnAudioData += EventSource_OnAudioData;

            UIUpdateTimer = new System.Timers.Timer(2000);
            UIUpdateTimer.Start();
            UIUpdateTimer.Elapsed += UIUpdateTimer_Elapsed;

        }

        static TimeSpan PlayerHideTimeout = TimeSpan.FromMinutes(0.25);
        static TimeSpan PlayerRemoveTimeout = TimeSpan.FromMinutes(1.0);
        void UIUpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var now = DateTime.UtcNow;
                var toRemove = new List<UserAudioPlayerWPF>();
                foreach (var player in AudioPlayers.ToList())
                {
                    if (player.Player == null || player.Player.LastReceived < (now - PlayerRemoveTimeout))
                    {
                        RemovePlayer(player);
                    }
                    else if (player.Player.LastReceived < (now - PlayerHideTimeout))
                    {
                        player.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    else
                    {
                        player.Visibility = System.Windows.Visibility.Visible;
                    }
                }
            }));
        }

        private static void RemovePlayer(UserAudioPlayerWPF player)
        {
            AudioPlayers.Remove(player);
            player.Dispose();
        }

        private void InitializeOutputDevices()
        {
            OutputList.Clear();

            foreach (var device in DirectSoundOut.Devices)
            {
                OutputList.Add(new OutputDeviceModel(device));
            }

            for (int n = 0; n < WaveOut.DeviceCount; n++)
            {
                var capabilities = WaveOut.GetCapabilities(n);
                OutputList.Add(new OutputDeviceModel(n, capabilities));
            }

        }

        private void LoadSettings()
        {
            var settings = Settings.Container;

            string strOutputDeviceGuid = settings.OutputDeviceGuid;
            var selectInputDevice = OutputList.Where(m => m.Guid.ToString() == strOutputDeviceGuid).FirstOrDefault();
            if (selectInputDevice != null)
                SelectedOutput = selectInputDevice;
            else
                SelectedOutput = OutputList.FirstOrDefault();
        }

        private void Settings_SaveSettings(object sender, EventArgs e)
        {
            var settings = Settings.Container;

            var selectedOut = SelectedOutput;
            if (selectedOut != null)
                settings.OutputDeviceGuid = selectedOut.Guid.ToString();
            else
                settings.OutputDeviceGuid = null;
        }

        void EventSource_OnAudioData(object sender, Client.AudioDataEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action<Guid, Audio.Codecs.CodecID, byte[]>(HandleAudio), e.UserID, e.Codec, e.Data);
        }

        private void HandleAudio(Guid userID, Audio.Codecs.CodecID codecid, byte[] encoded)
        {
            if (isDisposed) return;

            var player = GetOrCreateUserPlayer(userID);
            player.Player.HandleAudio(codecid, encoded);
        }

        private UserAudioPlayerWPF GetOrCreateUserPlayer(Guid userID)
        {
            UserAudioPlayerWPF toReturn;

            if (!AudioPlayers.Any(m => m.UserID == userID))
            {
                toReturn = new UserAudioPlayerWPF(userID, this);
                UIUpdateTimer.Elapsed += toReturn.UIUpdateTimer_Elapsed;
                AudioPlayers.Add(toReturn);
            }
            else
                toReturn = AudioPlayers.Single(m => m.UserID == userID);
            return toReturn;
        }

        bool isDisposed = false;
        public void Dispose()
        {
            foreach (var player in AudioPlayers)
                player.Dispose();
            AudioPlayers.Clear();
            isDisposed = true;
        }
    }
}
