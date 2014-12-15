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
    public partial class AudioOutWPF : UserControl
    {
        static ObservableCollection<OutputDeviceModel> OutputList = new ObservableCollection<OutputDeviceModel>();

        [ImportMany(typeof(Audio.Codecs.INetworkChatCodec))]
        public IEnumerable<Audio.Codecs.INetworkChatCodec> Codecs { get; set; }

        public static DependencyProperty OutputListProperty = DependencyProperty.Register("OutputList", typeof(ObservableCollection<OutputDeviceModel>), typeof(AudioOutWPF), new PropertyMetadata(OutputList));
        public static DependencyProperty SelectedOutputProperty = DependencyProperty.Register("SelectedOutput", typeof(OutputDeviceModel), typeof(AudioOutWPF), new PropertyMetadata(null, OnOutputDeviceChanged));
        public static DependencyProperty SelectedOutputIndexProperty = DependencyProperty.Register("SelectedOutputIndex", typeof(int?), typeof(AudioOutWPF), new PropertyMetadata(-1));

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
                target.Stop();
        }

        public static DependencyProperty OutputFormatProperty = DependencyProperty.Register("OutputFormat", typeof(string), typeof(AudioOutWPF), new PropertyMetadata(null));
        public string OutputFormat
        {
            get { return (string)this.GetValue(OutputFormatProperty); }
            set { SetValue(OutputFormatProperty, value); }
        }

        public static DependencyProperty CodecNameProperty = DependencyProperty.Register("CodecName", typeof(string), typeof(AudioOutWPF), new PropertyMetadata(null));
        public string CodecName
        {
            get { return (string)this.GetValue(CodecNameProperty); }
            set { SetValue(CodecNameProperty, value); }
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
                SetValue(BufferedDurationProperty, (int)value.TotalMilliseconds);
                SetValue(BufferedDurationStringProperty, string.Format("{0}ms", value.TotalMilliseconds));
            }
        }


        static int DefaultMaxBufferedDurationMs = 1500;
        static ushort DefaultSilenceAggression = 5;

        static int DefaultBufferTargetMs = 150;
        static int BufferTargetMarginMs = 50;

        static int BufferWarningDurationMs = 250;
        static int BufferCriticalDurationMs = 1000;

        public static DependencyProperty MaxBufferedDurationDurationProperty = DependencyProperty.Register("MaxBufferedDuration", typeof(int), typeof(AudioOutWPF), new PropertyMetadata(DefaultMaxBufferedDurationMs));
        TimeSpan _MaxBufferedDuration = TimeSpan.FromMilliseconds(DefaultMaxBufferedDurationMs);
        public TimeSpan MaxBufferedDuration
        {
            get { return _MaxBufferedDuration; }
            set
            {
                _MaxBufferedDuration = value;
                SetValue(MaxBufferedDurationDurationProperty, (int)value.TotalMilliseconds);
            }
        }

        public static DependencyProperty DroppedPacketsProperty = DependencyProperty.Register("DroppedPackets", typeof(int), typeof(AudioOutWPF), new PropertyMetadata(0));
        public int DroppedPackets
        {
            get { return (int)this.GetValue(DroppedPacketsProperty); }
            set { SetValue(DroppedPacketsProperty, value); }
        }

        public static DependencyProperty DroppedSilenceProperty = DependencyProperty.Register("DroppedSilence", typeof(int), typeof(AudioOutWPF), new PropertyMetadata(0));
        public int DroppedSilence
        {
            get { return (int)this.GetValue(DroppedSilenceProperty); }
            set { SetValue(DroppedSilenceProperty, value); }
        }

        public static DependencyProperty AddedSilenceProperty = DependencyProperty.Register("AddedSilence", typeof(int), typeof(AudioOutWPF), new PropertyMetadata(0));
        public int AddedSilence
        {
            get { return (int)this.GetValue(AddedSilenceProperty); }
            set { SetValue(AddedSilenceProperty, value); }
        }

        public static DependencyProperty BufferTargetProperty = DependencyProperty.Register("BufferTarget", typeof(int), typeof(AudioOutWPF), new PropertyMetadata(DefaultBufferTargetMs, OnBufferTargetPropertyUpdated));
        int bufferTarget = DefaultBufferTargetMs;
        public int BufferTarget
        {
            get { return bufferTarget; }
            set { bufferTarget = value; SetValue(BufferTargetProperty, value); }
        }
        private static void OnBufferTargetPropertyUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = d as AudioOutWPF;
            if (d != null)
                target.bufferTarget = (int)e.NewValue;
        }

        public static DependencyProperty SilenceAggressionProperty = DependencyProperty.Register("SilenceAggression", typeof(ushort), typeof(AudioOutWPF), new PropertyMetadata(DefaultSilenceAggression, OnSilenceAggressionUpdated));
        ushort silenceAggression = DefaultSilenceAggression;
        public ushort SilenceAggression
        {
            get { return silenceAggression; }
            set { silenceAggression = value; SetValue(SilenceAggressionProperty, value); }
        }
        private static void OnSilenceAggressionUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = d as AudioOutWPF;
            if (d != null)
                target.silenceAggression = (ushort)e.NewValue;
        }

        public static DependencyProperty UnderRunsProperty = DependencyProperty.Register("UnderRuns", typeof(int), typeof(AudioOutWPF), new PropertyMetadata(0));
        public int UnderRuns
        {
            get { return (int)this.GetValue(UnderRunsProperty); }
            set { SetValue(UnderRunsProperty, value); }
        }

        public static DependencyProperty MinProperty = DependencyProperty.Register("Min", typeof(int), typeof(AudioOutWPF));
        float _Min;
        public float Min
        {
            get { return _Min; }
            set { _Min = value; SetValue(MinProperty, (int)(value * 100)); }
        }

        public static DependencyProperty MaxProperty = DependencyProperty.Register("Max", typeof(int), typeof(AudioOutWPF));
        float _Max;
        public float Max
        {
            get { return _Max; }
            set { _Max = value; SetValue(MaxProperty, (int)(value * 100)); }
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
            Dispatcher.BeginInvoke(new Action<object, FftEventArgs>((s, args) =>
            {
                //if (this.selectedVisualization != null)
                //{
                //    this.selectedVisualization.OnFftCalculated(e.Result);
                //}
                //spectrumAnalyser.Update(e.Result);
            }), sender, e);
        }

        void audioGraph_MaximumCalculated(object sender, MaxSampleEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action<object, MaxSampleEventArgs>((s, args) =>
            {
                Min = args.MinSample;
                Max = args.MaxSample;

                //if (this.selectedVisualization != null)
                //{
                //    this.selectedVisualization.OnMaxCalculated(e.MinSample, e.MaxSample);
                //}
            }), sender, e);
        }


        private bool shouldUpdateDuration = false;
        void UIUpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            shouldUpdateDuration = true;
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
            ReceivedAudio(e.Codec, e.Data);
        }

        private IWavePlayer waveOut;
        private BufferedWaveProvider waveProvider;
        Audio.Codecs.CodecID? LastCodec = null;
        SampleChannel sampleChannel;
        NotifyingSampleProvider sampleStream;

        int droppedPackets = 0;
        int droppedSilence = 0;
        int addedSilence = 0;
        int underruns = 0;

        private byte addSilenceThreshold { get { return (byte)(silenceAggression * 10); } }

        public bool ShouldDropSilence { get { return silenceAggression > 0 && BufferedDuration.TotalMilliseconds > (bufferTarget + BufferTargetMarginMs); } }
        public bool ShouldAddSilence { get { return silenceAggression > 0 && BufferedDuration.TotalMilliseconds < (bufferTarget); } }
        public ushort silenceThreshhold
        {
            get
            {
                int duration = (int)BufferedDuration.TotalMilliseconds;
                if (duration > BufferCriticalDurationMs)
                    return (ushort)(12 * silenceAggression);
                if (duration > BufferWarningDurationMs)
                    return (ushort)(6 * silenceAggression);
                return (ushort)(3 * silenceAggression);
            }

        }

        private void ReceivedAudio(Audio.Codecs.CodecID codecid, byte[] encoded)
        {
            Dispatcher.BeginInvoke(new Action<Audio.Codecs.CodecID, byte[]>(HandleAudio), codecid, encoded);
        }

        private void HandleAudio(Audio.Codecs.CodecID codecid, byte[] encoded)
        {
            Audio.Codecs.INetworkChatCodec remoteCodec = Codecs.SingleOrDefault(m => m.CodecID == codecid);
            if (remoteCodec == null)
            {
                Console.WriteLine("Bad Audio Packet: Codec ID {0}", codecid);
                return;
            }
            if (codecid != LastCodec)
            {
                LastCodec = codecid;
                CodecName = remoteCodec.Name();
            }

            if (waveOut != null && waveProvider.WaveFormat != remoteCodec.RecordFormat)
                Stop();

            if (waveOut == null)
                Start(remoteCodec);

            TimeSpan buffered = waveProvider.BufferedDuration;

            if (buffered == TimeSpan.Zero) UnderRuns = underruns++;

            if (buffered <= MaxBufferedDuration)
            {
                byte[] decoded = remoteCodec.Decode(encoded, encoded.Length);
                int length = decoded.Length;

                if (ShouldDropSilence)
                {
                    int dropped = DropSilence(silenceThreshhold, ref decoded, ref length);
                    DroppedSilence = droppedSilence += dropped;
                }
                else if (ShouldAddSilence && length > 5)
                {
                    bool silent = true;
                    for (int i = 0; i < 5; i += 2)
                    {
                        if (decoded[i + 1] != 0 || decoded[i] > addSilenceThreshold)
                        {
                            //  if (i > 5)
                            //Console.WriteLine("sil:{0} {1} {2}", i, decoded[i + 1], decoded[i]);
                            silent = false;
                            break;
                        }
                    }
                    if (silent)
                    {
                        var silence = new byte[length];
                        byte silenceLevel = (byte)(addSilenceThreshold / 2);
                        for (int i = 0; i < length; i += 2)
                        {
                            silence[i + 1] = 0;
                            silence[i] = silenceLevel;
                        }
                        waveProvider.AddSamples(silence, 0, length);
                        AddedSilence = addedSilence += length;
                    }
                }

                waveProvider.AddSamples(decoded, 0, length);
            }
            else
                DroppedPackets = ++droppedPackets;

            if (shouldUpdateDuration)
            {
                BufferedDuration = buffered;
                shouldUpdateDuration = false;
            }
        }

        private void Start(Audio.Codecs.INetworkChatCodec codec)
        {
            waveOut = GetWavePlayer();

            waveProvider = new BufferedWaveProvider(codec.RecordFormat);

            sampleChannel = new SampleChannel(waveProvider, false);
            sampleStream = new NotifyingSampleProvider(sampleChannel);
            sampleStream.Sample += (s, e) => aggregator.Add(e.Left);
            waveOut.Init(sampleStream);
            waveOut.Play();

            OutputFormat = codec.RecordFormat.ToString();
        }

        private void Stop()
        {
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }
            waveProvider = null;
            sampleChannel = null;
            sampleStream = null;
        }

        private IWavePlayer GetWavePlayer()
        {
            int directSoundLatency = 40;

            switch (SelectedOutput.Provider)
            {
                case OutputDeviceModel.OutputDeviceProvider.Wave:
                    return new WaveOut() { DeviceNumber = SelectedOutput.WavDeviceNumber };
                case OutputDeviceModel.OutputDeviceProvider.DirectSound:
                    return new DirectSoundOut(directSoundLatency);
            }
            return new DirectSoundOut(DirectSoundOut.DSDEVID_DefaultVoicePlayback, directSoundLatency);
        }

        private static int DropSilence(ushort silenceThreshhold, ref byte[] decoded, ref int length)
        {
            int dropped = 0;
            var erg = new byte[length];
            int j = 0;
            erg[j++] = decoded[0];
            erg[j++] = decoded[1];
            for (int i = 2; i < length; i += 2)
            {
                if (i + 6 < length)
                {
                    var sample0 = (ushort)(decoded[i - 2] | (decoded[i - 1] << 8));
                    var sample1 = (ushort)(decoded[i] | (decoded[i + 1] << 8));
                    var sample2 = (ushort)(decoded[i + 2] | (decoded[i + 3] << 8));
                    if (sample0 < silenceThreshhold && sample1 < silenceThreshhold && sample2 < silenceThreshhold)
                    {
                        dropped++;
                        continue;
                    }
                }
                erg[j++] = decoded[i];
                erg[j++] = decoded[i + 1];
            }
            length = j;
            decoded = erg;
            return dropped;
        }
    }
}
