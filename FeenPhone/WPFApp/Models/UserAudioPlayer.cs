using FeenPhone.Audio;
using FeenPhone.WPFApp.Controls;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FeenPhone.WPFApp.Models
{
    public class UserAudioPlayer : DependencyObject, IDisposable
    {
        public DateTime LastReceived { get; set; }

        AudioOutWPF Parent;
        public Guid UserID { get; private set; }

        Audio.Codecs.CodecID? LastCodec = null;
        SampleChannel sampleChannel;
        NotifyingSampleProvider sampleStream;

        private IWavePlayer waveOut;
        private BufferedWaveProvider waveProvider;

        public UserAudioPlayer(Guid userID, AudioOutWPF parent)
        {
            Parent = parent;
            this.UserID = userID;

            aggregator = new FeenPhone.Audio.SampleAggregator();
            aggregator.NotificationCount = 882;
            aggregator.PerformFFT = true;

            MaximumCalculated += new EventHandler<MaxSampleEventArgs>(audioGraph_MaximumCalculated);
            FftCalculated += new EventHandler<FftEventArgs>(audioGraph_FftCalculated);

            LastReceived = DateTime.UtcNow;
        }

        public void UpdateLastReceived(DateTime now)
        {
            LastReceived = now;
        }

        static int DefaultMaxBufferedDurationMs = 1500;
        static ushort DefaultSilenceAggression = 0;

        static int DefaultBufferTargetMs = 50;
        static int BufferTargetMarginMs = 50;

        static int BufferWarningDurationMs = 250;
        static int BufferCriticalDurationMs = 1000;

        public static DependencyProperty CodecNameProperty = DependencyProperty.Register("CodecName", typeof(string), typeof(UserAudioPlayer), new PropertyMetadata(null));
        public string CodecName
        {
            get { return (string)this.GetValue(CodecNameProperty); }
            set { SetValue(CodecNameProperty, value); }
        }

        public static DependencyProperty OutputFormatProperty = DependencyProperty.Register("OutputFormat", typeof(string), typeof(UserAudioPlayer), new PropertyMetadata(null));
        public string OutputFormat
        {
            get { return (string)this.GetValue(OutputFormatProperty); }
            set { SetValue(OutputFormatProperty, value); }
        }

        public static DependencyProperty UnderRunsProperty = DependencyProperty.Register("UnderRuns", typeof(int), typeof(UserAudioPlayer), new PropertyMetadata(0));
        public int UnderRuns
        {
            get { return (int)this.GetValue(UnderRunsProperty); }
            set { SetValue(UnderRunsProperty, value); }
        }

        public static DependencyProperty MinProperty = DependencyProperty.Register("Min", typeof(int), typeof(UserAudioPlayer));
        float _MinUnscaled;
        public float Min
        {
            get { return _MinUnscaled; }
            set { _MinUnscaled = value; SetValue(MinProperty, (int)(value * 100)); }
        }

        public static DependencyProperty MaxProperty = DependencyProperty.Register("Max", typeof(int), typeof(UserAudioPlayer));
        float _MaxUnscaled;
        public float Max
        {
            get { return _MaxUnscaled; }
            set { _MaxUnscaled = value; SetValue(MaxProperty, (int)(value * 100)); }
        }
        public static DependencyProperty BufferedDurationStringProperty = DependencyProperty.Register("BufferedDurationString", typeof(string), typeof(UserAudioPlayer), new PropertyMetadata(null));
        public static DependencyProperty BufferedDurationProperty = DependencyProperty.Register("BufferedDurationMs", typeof(int), typeof(UserAudioPlayer), new PropertyMetadata(0));
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

        public static DependencyProperty MaxBufferedDurationDurationProperty = DependencyProperty.Register("MaxBufferedDurationMs", typeof(int), typeof(UserAudioPlayer), new PropertyMetadata(DefaultMaxBufferedDurationMs));
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

        public static DependencyProperty DroppedPacketsProperty = DependencyProperty.Register("DroppedPackets", typeof(int), typeof(UserAudioPlayer), new PropertyMetadata(0));
        public int DroppedPackets
        {
            get { return (int)this.GetValue(DroppedPacketsProperty); }
            set { SetValue(DroppedPacketsProperty, value); }
        }

        public static DependencyProperty DroppedSilenceProperty = DependencyProperty.Register("DroppedSilence", typeof(int), typeof(UserAudioPlayer), new PropertyMetadata(0));
        public int DroppedSilence
        {
            get { return (int)this.GetValue(DroppedSilenceProperty); }
            set { SetValue(DroppedSilenceProperty, value); }
        }

        public static DependencyProperty AddedSilenceProperty = DependencyProperty.Register("AddedSilence", typeof(int), typeof(UserAudioPlayer), new PropertyMetadata(0));
        public int AddedSilence
        {
            get { return (int)this.GetValue(AddedSilenceProperty); }
            set { SetValue(AddedSilenceProperty, value); }
        }

        public static DependencyProperty BufferTargetProperty = DependencyProperty.Register("BufferTarget", typeof(int), typeof(UserAudioPlayer), new PropertyMetadata(DefaultBufferTargetMs, OnBufferTargetPropertyUpdated));
        int bufferTarget = DefaultBufferTargetMs;
        public int BufferTarget
        {
            get { return bufferTarget; }
            set { bufferTarget = value; SetValue(BufferTargetProperty, value); }
        }
        private static void OnBufferTargetPropertyUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = d as UserAudioPlayer;
            if (d != null)
                target.bufferTarget = (int)e.NewValue;
        }

        public static DependencyProperty SilenceAggressionProperty = DependencyProperty.Register("SilenceAggression", typeof(ushort), typeof(UserAudioPlayer), new PropertyMetadata(DefaultSilenceAggression, OnSilenceAggressionUpdated));
        ushort silenceAggression = DefaultSilenceAggression;
        public ushort SilenceAggression
        {
            get { return silenceAggression; }
            set { silenceAggression = value; SetValue(SilenceAggressionProperty, value); }
        }
        private static void OnSilenceAggressionUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = d as UserAudioPlayer;
            if (d != null)
                target.silenceAggression = (ushort)e.NewValue;
        }

        private bool shouldUpdateDuration = false;
        internal void UIUpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            shouldUpdateDuration = true;
        }

        public void HandleAudio(Audio.Codecs.CodecID codecid, byte[] encoded)
        {
            if (isDisposed) return;

            Audio.Codecs.INetworkChatCodec remoteCodec = Parent.Codecs.SingleOrDefault(m => m.CodecID == codecid);
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

            if (buffered == TimeSpan.Zero) UnderRuns++;

            if (buffered <= MaxBufferedDuration)
            {
                byte[] decoded = remoteCodec.Decode(encoded, encoded.Length);
                int length = decoded.Length;

                if (ShouldDropSilence)
                {
                    int dropped = DropSilence(silenceThreshhold, ref decoded, ref length);
                    DroppedSilence += dropped;
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
                        AddedSilence += length;
                    }
                }

                waveProvider.AddSamples(decoded, 0, length);
            }
            else
                DroppedPackets++;

            if (shouldUpdateDuration)
            {
                BufferedDuration = buffered;
                shouldUpdateDuration = false;
            }
            LastReceived = DateTime.UtcNow;
        }

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

        int desiredLatency = 150;

        private bool useEventSync = true;
        public AudioClientShareMode shareMode = AudioClientShareMode.Shared;

        private IWavePlayer GetWavePlayer()
        {

            var SelectedOutput = Parent.SelectedOutput;
            switch (SelectedOutput.Provider)
            {
                case DeviceModel.DeviceProvider.Wave:
                    return new WaveOut() { DeviceNumber = SelectedOutput.WavDeviceNumber, DesiredLatency = desiredLatency };
                case DeviceModel.DeviceProvider.DirectSound:
                    return new DirectSoundOut(SelectedOutput.DirectSoundDeviceInfo.Guid, desiredLatency);
                case DeviceModel.DeviceProvider.Wasapi:
                    return new WasapiOut(SelectedOutput.MMDevice, shareMode, useEventSync, desiredLatency);
            }
            return new DirectSoundOut(DirectSoundOut.DSDEVID_DefaultVoicePlayback, desiredLatency);
        }

        internal void Stop()
        {
            if (waveOut != null)
            {
                waveOut.Stop();
                try
                {
                    waveOut.Dispose();
                }
                catch { }
                waveOut = null;
            }
            waveProvider = null;
            sampleChannel = null;
            sampleStream = null;
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


        bool isDisposed = false;
        public void Dispose()
        {
            Stop();
            isDisposed = true;
        }
    }
}
