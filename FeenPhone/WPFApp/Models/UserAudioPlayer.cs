﻿using Alienseed.BaseNetworkServer.Accounting;
using FeenPhone.Audio;
using FeenPhone.WPFApp.Controls;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FeenPhone.WPFApp.Models
{
    public class UserAudioPlayer : DependencyObject, IDisposable
    {
        public AudioVisualizationSource VisSource { get; private set; }
        private readonly FeenPhone.Audio.SampleAggregator aggregator;

        [ImportMany(typeof(Audio.Codecs.INetworkChatCodec))]
        public IEnumerable<Audio.Codecs.INetworkChatCodec> Codecs { get; set; }

        public DateTime LastReceived { get; set; }

        public AudioOutWPF Parent { get; private set; }
        public Guid UserID { get; private set; }

        public static DependencyProperty UserProperty = DependencyProperty.Register("User", typeof(UserStatusModel), typeof(UserAudioPlayer), new PropertyMetadata(null));
        public UserStatusModel User
        {
            get { return (UserStatusModel)this.GetValue(UserProperty); }
            set { this.SetValue(UserProperty, value); }
        }

        Audio.Codecs.CodecID? LastCodec = null;
        SampleChannel sampleChannel;
        NotifyingSampleProvider sampleStream;

        private IWavePlayer waveOut;
        private BufferedWaveProvider waveProvider;

        public UserAudioPlayer(Guid userID, AudioOutWPF parent, bool useWaveEvent = true)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
                new CompositionContainer(new AssemblyCatalog(Assembly.GetExecutingAssembly())).ComposeParts(this);

            Parent = parent;
            this.UserID = userID;
            UseWaveEvent = useWaveEvent;

            User = UserStatusRepo.FindUser(userID);
            UserStatusRepo.Users.CollectionChanged += Users_CollectionChanged;

            aggregator = new FeenPhone.Audio.SampleAggregator() { PerformFFT = false };
            aggregator.NotificationCount = 882;
            aggregator.PerformFFT = true;

            VisSource = new AudioVisualizationSource(aggregator);

            LastReceived = DateTime.UtcNow;
        }

        void Users_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var found = false;
            if (UserStatusRepo.Users != null)
            {
                foreach (var user in UserStatusRepo.Users)
                    if (user.ID == UserID)
                    {
                        User = user;
                        found = true;
                        break;
                    }
            }
            if (!found)
                User = null;
        }

        public void UpdateLastReceived(DateTime now)
        {
            LastReceived = now;
        }

        static int DefaultMaxBufferedDurationMs = 500;
        TimeSpan FrameDropThresholdMs { get { return MaxBufferedDuration.Add(MaxBufferedDuration); } }
        static ushort DefaultSilenceAggression = 1;

        static int DefaultBufferTargetMs = 100;
        static int BufferTargetMarginMs = 50;

        static int BufferWarningDurationMs = 150;
        static int BufferCriticalDurationMs = 300;

        public static DependencyProperty UseWaveEventProperty = DependencyProperty.Register("UseWaveEvent", typeof(bool), typeof(UserAudioPlayer), new PropertyMetadata(true, OnUseWaveEventChanged));
        public bool UseWaveEvent
        {
            get { return (bool)GetValue(UseWaveEventProperty); }
            set { SetValue(UseWaveEventProperty, value); }
        }
        private static void OnUseWaveEventChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((UserAudioPlayer)d).Stop();
        }

        public static DependencyProperty LevelManagerProperty = DependencyProperty.Register("LevelManager", typeof(AudioLevelManager), typeof(UserAudioPlayer), new PropertyMetadata(null));
        private AudioLevelManager LevelManager
        {
            get { return (AudioLevelManager)this.GetValue(LevelManagerProperty); }
            set { this.SetValue(LevelManagerProperty, value); }
        }

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

        public static DependencyProperty BufferedDurationProperty = DependencyProperty.Register("BufferedDurationMs", typeof(int), typeof(UserAudioPlayer), new PropertyMetadata(0));
        TimeSpan _BufferedDuration = TimeSpan.Zero;
        public TimeSpan BufferedDuration
        {
            get { return _BufferedDuration; }
            set
            {
                _BufferedDuration = value;
                SetValue(BufferedDurationProperty, (int)value.TotalMilliseconds);
            }
        }

        public int MinBufferedDurationMs { get { return 50; } }

        public static DependencyProperty MaxBufferedDurationMsProperty = DependencyProperty.Register("MaxBufferedDurationMs", typeof(int), typeof(UserAudioPlayer), new PropertyMetadata(DefaultMaxBufferedDurationMs));
        TimeSpan _MaxBufferedDuration = TimeSpan.FromMilliseconds(DefaultMaxBufferedDurationMs);
        public TimeSpan MaxBufferedDuration
        {
            get { return _MaxBufferedDuration; }
            set
            {
                _MaxBufferedDuration = value;
                SetValue(MaxBufferedDurationMsProperty, (int)value.TotalMilliseconds);
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
        public int BufferTarget
        {
            get { return (int)GetValue(BufferTargetProperty); }
            set { SetValue(BufferTargetProperty, value); }
        }

        int bufferTarget = DefaultBufferTargetMs;
        private static void OnBufferTargetPropertyUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = d as UserAudioPlayer;
            if (target != null)
            {
                target.bufferTarget = (int)e.NewValue;
                target.TryUpdateLatency();
            }
        }

        ushort silenceAggression = DefaultSilenceAggression;
        public static DependencyProperty BufferRecoveryEnabledProperty = DependencyProperty.Register("BufferRecoveryEnabled", typeof(bool), typeof(UserAudioPlayer), new PropertyMetadata(DefaultSilenceAggression != 0, OnBufferRecoveryEnabledChanged));
        public bool BufferRecoveryEnabled
        {
            get { return (bool)GetValue(BufferRecoveryEnabledProperty); }
            set { SetValue(BufferRecoveryEnabledProperty, value); }
        }
        private static void OnBufferRecoveryEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = d as UserAudioPlayer;
            if (target != null)
            {
                bool newValue = (bool)e.NewValue;
                target.silenceAggression = newValue ? DefaultSilenceAggression : (ushort)0;
            }
        }

        private bool shouldUpdateDuration = false;
        internal void UIUpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            shouldUpdateDuration = true;
        }

        volatile bool ShouldTryRestartOutput = false;
        private void TryUpdateLatency()
        {
            ShouldTryRestartOutput = (waveOut != null && waveOut.PlaybackState == PlaybackState.Playing);
        }

        public void HandleAudio(Audio.Codecs.CodecID codecid, byte[] encoded)
        {
            if (isDisposed) return;

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

            TimeSpan buffered = waveOut == null ? TimeSpan.Zero : waveProvider.BufferedDuration;
            bool isPlaying = buffered != TimeSpan.Zero;

            if (waveOut == null || waveProvider.WaveFormat != remoteCodec.RecordFormat || (!isPlaying && ShouldTryRestartOutput))
            {
                Start(remoteCodec);
            }
            else if (!isPlaying)
            {
                UnderRuns++;
            }

            if (buffered <= FrameDropThresholdMs)
            {
                byte[] decoded = remoteCodec.Decode(encoded, encoded.Length);
                int length = decoded.Length;

                if (!isPlaying && AudioOutWPF.shouldRampUnderruns)
                {
                    InputResampler.RampPCM16Volume(ref decoded, length, InputResampler.RampDirection.ZeroToFull);
                }

                var volumeScalar = LevelManager.LevelScalar;
                if (volumeScalar < 1.0)
                {
                    InputResampler.ScalePCM16Volume(ref decoded, length, volumeScalar);
                }

                if (length > 0 && ShouldDropSilence)
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
                            silent = false;
                            break;
                        }
                    }
                    if (silent)
                    {
                        var silenceBytes = length / 4;
                        var silence = new byte[silenceBytes];
                        byte silenceLevel = (byte)(addSilenceThreshold / 2);
                        for (int i = 0; i < silenceBytes - 1; i += 2)
                        {
                            silence[i + 1] = 0;
                            silence[i] = silenceLevel;
                        }
                        waveProvider.AddSamples(silence, 0, silenceBytes);
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
        public bool ShouldAddSilence { get { return false /*silenceAggression > 0 && BufferedDuration.TotalMilliseconds < (bufferTarget)*/; } }
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
            if (length <= 5) return 0;

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
            ShouldTryRestartOutput = false;

            Stop();

            waveOut = GetWavePlayer();

            waveProvider = new BufferedWaveProvider(codec.RecordFormat);

            sampleChannel = new SampleChannel(waveProvider, false);
            sampleStream = new NotifyingSampleProvider(sampleChannel);
            sampleStream.Sample += (s, e) => aggregator.Add(e.Left);
            waveOut.Init(sampleStream);
            waveOut.Play();

            LevelManager = new AudioLevelManagerDisconnected();

            OutputFormat = codec.RecordFormat.ToString();
        }

        public AudioClientShareMode shareMode = AudioClientShareMode.Shared;

        private IWavePlayer GetWavePlayer()
        {
            int desiredLatency = Math.Max(MinBufferedDurationMs, Math.Min((int)MaxBufferedDuration.TotalMilliseconds, BufferTarget));

            var SelectedOutput = Parent.SelectedOutput;

            Console.WriteLine("Initializing Output for {0} using {1}{2} with latency of {3}ms", User == null ? "<new user>" : User.Nickname ?? User.ID.ToString(), SelectedOutput.Provider, SelectedOutput.Provider == DeviceProvider.Wave && UseWaveEvent ? "Event" : "", desiredLatency);

            return AudioOutWPF.InstanciateWavePlayerForOutput(SelectedOutput, desiredLatency, shareMode, UseWaveEvent);
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


        bool isDisposed = false;
        public void Dispose()
        {
            Stop();
            isDisposed = true;
        }
    }
}
