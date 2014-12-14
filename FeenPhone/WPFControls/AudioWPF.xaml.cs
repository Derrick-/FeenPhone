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
using NAudio.Wave;
using System.Collections.ObjectModel;
using System.ComponentModel;
using FeenPhone.Audio;

namespace FeenPhone.WPFControls
{
    /// <summary>
    /// Interaction logic for AudioWPF.xaml
    /// </summary>
    public partial class AudioWPF : UserControl, INotifyPropertyChanged
    {
        static ObservableCollection<string> InputList = new ObservableCollection<string>();

        public AudioWPF()
        {
            InitializeComponent();
            DataContext = this;
            InitializeInputDevices();
        }

        private void InitializeInputDevices()
        {
            InputList.Clear();
            for (int n = 0; n < WaveIn.DeviceCount; n++)
            {
                var capabilities = WaveIn.GetCapabilities(n);
                InputList.Add(capabilities.ProductName);
            }
        }

        public static DependencyProperty IsRecordingProperty = DependencyProperty.Register("IsRecording", typeof(bool?), typeof(AudioWPF), new PropertyMetadata(false, OnIsRecordingChanged));
        public static DependencyProperty InputSourceListProperty = DependencyProperty.Register("InputSourceList", typeof(ObservableCollection<string>), typeof(AudioWPF), new PropertyMetadata(InputList));
        public static DependencyProperty SelectedInputSourceProperty = DependencyProperty.Register("SelectedInputSource", typeof(string), typeof(AudioWPF), new PropertyMetadata(null));
        public static DependencyProperty SelectedInputSourceIndexProperty = DependencyProperty.Register("SelectedInputSourceIndex", typeof(int?), typeof(AudioWPF), new PropertyMetadata(null));

        public bool? IsRecording
        {
            get { return (bool?)this.GetValue(IsRecordingProperty); }
            set
            {
                this.SetValue(IsRecordingProperty, value);
                OnPropertyChanged("IsRecording");
            }
        }

        public int? SelectedInputSourceIndex
        {
            get { return (int?)this.GetValue(SelectedInputSourceIndexProperty); }
            set { this.SetValue(SelectedInputSourceIndexProperty, value); }
        }

        private static void OnIsRecordingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AudioWPF target = d as AudioWPF;
            if (target != null)
            {
                target.SetRecording((bool?)e.NewValue == true);
            }
        }

        private void SetRecording(bool enabled)
        {
            if (enabled)
                StartRecording();
            else
                StopRecording();
        }

        private WaveIn waveIn;
        private INetworkChatCodec codec = new Audio.MicrosoftAdpcmChatCodec();
        private void StartRecording()
        {
            if (SelectedInputSourceIndex.HasValue)
            {
                int source = SelectedInputSourceIndex.Value;
                waveIn = new WaveIn();
                waveIn.BufferMilliseconds = 50;
                waveIn.DeviceNumber = source;
                waveIn.WaveFormat = codec.RecordFormat;
                waveIn.DataAvailable += waveIn_DataAvailable;
                waveIn.StartRecording();
            }
            else
                IsRecording = false;
        }

        private void StopRecording()
        {
            waveIn.DataAvailable -= waveIn_DataAvailable;
            waveIn.StopRecording();
        }

        private void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if(IsEnabled)
            {
                byte[] encoded = codec.Encode(e.Buffer, 0, e.BytesRecorded);

                ReceivedAudio(encoded);
                //if (NetworkWPF.Client != null)
                //    NetworkWPF.Client.SendAudio(text);
            }
        }

        private IWavePlayer waveOut;
        private BufferedWaveProvider waveProvider;
        static readonly TimeSpan MaxBufferedDuration = TimeSpan.FromMilliseconds(100);
        private void ReceivedAudio(byte[] encoded)
        {
            if (waveOut == null)
            {
                waveOut = new DirectSoundOut(50);
                waveProvider = new BufferedWaveProvider(codec.RecordFormat);
                waveOut.Init(waveProvider);
                waveOut.Play();
            }
            if (waveProvider.BufferedDuration <= MaxBufferedDuration)
            {
                byte[] decoded = codec.Decode(encoded, 0, encoded.Length);
                waveProvider.AddSamples(decoded, 0, decoded.Length);
            }
            else
                Console.WriteLine("Skipping audio data to reduce latency");
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                Dispatcher.BeginInvoke(new Action(() => { propertyChanged(this, new PropertyChangedEventArgs(propertyName)); }));
            }
        }

    }
}
