using FeenPhone.Audio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
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
    public partial class AudioOutWPF : UserControl
    {
        [ImportMany(typeof(Audio.Codecs.INetworkChatCodec))]
        public IEnumerable<Audio.Codecs.INetworkChatCodec> Codecs { get; set; }

        public static DependencyProperty OutputFormatProperty = DependencyProperty.Register("OutputFormat", typeof(string), typeof(AudioOutWPF), new PropertyMetadata(null));
        public string OutputFormat
        {
            get { return (string)this.GetValue(OutputFormatProperty); }
            set
            {
                Dispatcher.BeginInvoke(new Action<DependencyProperty, object>(SetValue), OutputFormatProperty, value);
            }
        }

        public static DependencyProperty CodecNameProperty = DependencyProperty.Register("CodecName", typeof(string), typeof(AudioOutWPF), new PropertyMetadata(null));
        public string CodecName
        {
            get { return (string)this.GetValue(CodecNameProperty); }
            set
            {
                Dispatcher.BeginInvoke(new Action<DependencyProperty, object>(SetValue), CodecNameProperty, value);
            }
        }

        public static DependencyProperty BufferedDurationStringProperty = DependencyProperty.Register("BufferedDurationString", typeof(string), typeof(AudioOutWPF), new PropertyMetadata(null));
        public static DependencyProperty BufferedDurationProperty = DependencyProperty.Register("BufferedDuration", typeof(int), typeof(AudioOutWPF), new PropertyMetadata(0));
        TimeSpan _BufferedDuration = TimeSpan.Zero;
        public TimeSpan BufferedDuration
        {
            get { return _BufferedDuration; }
            set
            {
                _BufferedDuration = value;
                Dispatcher.BeginInvoke(new Action<DependencyProperty, object>(SetValue), BufferedDurationProperty, (int)value.TotalMilliseconds);
                Dispatcher.BeginInvoke(new Action<DependencyProperty, object>(SetValue), BufferedDurationStringProperty, string.Format("{0}ms", value.TotalMilliseconds));
            }
        }

        static int DefaultMaxBufferedDurationMs = 250;
        public static DependencyProperty MaxBufferedDurationDurationProperty = DependencyProperty.Register("MaxBufferedDuration", typeof(int), typeof(AudioOutWPF), new PropertyMetadata(DefaultMaxBufferedDurationMs));
        TimeSpan _MaxBufferedDuration = TimeSpan.FromMilliseconds(DefaultMaxBufferedDurationMs);
        public TimeSpan MaxBufferedDuration
        {
            get { return _MaxBufferedDuration; }
            set
            {
                _MaxBufferedDuration = value;
                Dispatcher.BeginInvoke(new Action<DependencyProperty, object>(SetValue), MaxBufferedDurationDurationProperty, (int)value.TotalMilliseconds);
            }
        }

        public static DependencyProperty DroppedPacketsProperty = DependencyProperty.Register("DroppedPackets", typeof(int), typeof(AudioOutWPF), new PropertyMetadata(0));
        public int DroppedPackets
        {
            get { return (int)this.GetValue(DroppedPacketsProperty); }
            set
            {
                Dispatcher.BeginInvoke(new Action<DependencyProperty, object>(SetValue), DroppedPacketsProperty, value);
            }
        }

        public static DependencyProperty MinProperty = DependencyProperty.Register("Min", typeof(float), typeof(AudioOutWPF));
        public float Min
        {
            get { return (float)this.GetValue(MinProperty); }
            set { Dispatcher.BeginInvoke(new Action<DependencyProperty, object>(SetValue), MinProperty, value); }
        }

        public static DependencyProperty MaxProperty = DependencyProperty.Register("Max", typeof(int), typeof(AudioOutWPF));
        float _Max;
        public float Max
        {
            get { return _Max; }
            set { _Max = value; Dispatcher.BeginInvoke(new Action<DependencyProperty, object>(SetValue), MaxProperty, (int)(value*100)); }
        }


        System.Timers.Timer UIUpdateTimer;
        public AudioOutWPF()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
                new CompositionContainer(new AssemblyCatalog(Assembly.GetExecutingAssembly())).ComposeParts(this);

            InitializeComponent();
            DataContext = this;

            LoadSettings();
            Settings.SaveSettings += Settings_SaveSettings;

            FeenPhone.Client.EventSource.OnAudioData += EventSource_OnAudioData;

            UIUpdateTimer = new System.Timers.Timer(500);
            UIUpdateTimer.Start();
            UIUpdateTimer.Elapsed += UIUpdateTimer_Elapsed;

            aggregator = new FeenPhone.Audio.SampleAggregator();
            aggregator.NotificationCount = 882;
            aggregator.PerformFFT = true;

            MaximumCalculated += new EventHandler<MaxSampleEventArgs>(audioGraph_MaximumCalculated);
            FftCalculated += new EventHandler<FftEventArgs>(audioGraph_FftCalculated);
        }

        private readonly FeenPhone.Audio.SampleAggregator aggregator;
        public event EventHandler<FeenPhone.Audio.FftEventArgs> FftCalculated
        {
            add { aggregator.FftCalculated += value; }
            remove { aggregator.FftCalculated -= value; }
        }

        public event EventHandler<FeenPhone.Audio.MaxSampleEventArgs> MaximumCalculated
        {
            add { aggregator.MaximumCalculated += value; }
            remove { aggregator.MaximumCalculated -= value; }
        }

        void audioGraph_FftCalculated(object sender, FftEventArgs e)
        {
            //if (this.selectedVisualization != null)
            //{
            //    this.selectedVisualization.OnFftCalculated(e.Result);
            //}
            //spectrumAnalyser.Update(e.Result);
        }

        void audioGraph_MaximumCalculated(object sender, MaxSampleEventArgs e)
        {
            Min = e.MinSample;
            Max = e.MaxSample;

            //if (this.selectedVisualization != null)
            //{
            //    this.selectedVisualization.OnMaxCalculated(e.MinSample, e.MaxSample);
            //}
        }


        private bool shouldUpdateDuration = false;
        void UIUpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            shouldUpdateDuration = true;
        }

        private void LoadSettings()
        {
            var settings = Settings.Container;
        }

        private void Settings_SaveSettings(object sender, EventArgs e)
        {
            var settings = Settings.Container;
        }

        void EventSource_OnAudioData(object sender, Client.AudioDataEventArgs e)
        {
            ReceivedAudio(e.Codec, e.Data);
        }

        private IWavePlayer waveOut;
        private BufferedWaveProvider waveProvider;
        Audio.Codecs.CodecID? LastCodec = null;
        SampleChannel sampleChannel;
        NotifyingSampleProvider sampleStream;
        int droppedPackets = 0;

        private void ReceivedAudio(Audio.Codecs.CodecID codecid, byte[] encoded)
        {
            Audio.Codecs.INetworkChatCodec remoteCodec = Codecs.SingleOrDefault(m => m.CodecID == codecid);
            if (codecid != LastCodec)
            {
                LastCodec = codecid;
                CodecName = remoteCodec.Name();
            }

            if (waveOut != null && waveProvider.WaveFormat != remoteCodec.RecordFormat)
            {
                waveOut.Stop();
                waveOut = null;
            }

            if (waveOut == null)
            {
                waveOut = new DirectSoundOut(50);

                waveProvider = new BufferedWaveProvider(remoteCodec.RecordFormat);

                sampleChannel = new SampleChannel(waveProvider, false);
                sampleStream = new NotifyingSampleProvider(sampleChannel);
                sampleStream.Sample += (s, e) => aggregator.Add(e.Left);
                waveOut.Init(sampleStream);
                waveOut.Play();

                OutputFormat = remoteCodec.RecordFormat.ToString();
            }
            TimeSpan buffered = waveProvider.BufferedDuration;
            if (buffered <= MaxBufferedDuration)
            {
                byte[] decoded = remoteCodec.Decode(encoded, encoded.Length);
                waveProvider.AddSamples(decoded, 0, decoded.Length);
            }
            else
                DroppedPackets = ++droppedPackets;

            if (shouldUpdateDuration)
            {
                BufferedDuration = buffered;
                shouldUpdateDuration = false;
            }
        }
    }
}
