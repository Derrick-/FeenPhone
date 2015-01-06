using NAudio.CoreAudioApi;
using NAudio.Mixer;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace FeenPhone.WPFApp.Models
{
    class AudioInLevelManager : AudioLevelManager
    {
        private readonly WasapiCapture wasapiInDevice;

        public AudioInLevelManager(WaveIn waveDevice) : base(waveDevice)
        {
            LoadSettings();
        }
 
        public AudioInLevelManager(WaveInEvent waveDevice) : base(waveDevice)
        {
            LoadSettings();
        }

        public AudioInLevelManager(WasapiCapture waspicapture, MMDevice mmdevice)  : base(mmdevice, DeviceType.In)
        {
            LoadSettings();
            this.wasapiInDevice = waspicapture;
        }

        private void LoadSettings()
        {
            var settings = Settings.Container;
            if (settings.DefaultInputLevel > 0.0)
                Level = settings.DefaultInputLevel;
        }

        private void SaveSettings()
        {
            var settings = Settings.Container;
            settings.DefaultInputLevel = Level;
        }

        public override void Dispose()
        {
            if (!isDisposed)
                SaveSettings();
            base.Dispose();
        }
    }

    class AudioOutLevelManager : AudioLevelManager
    {
        public AudioOutLevelManager(MMDevice mmdevice) : base(mmdevice, DeviceType.Out) { }
    }

    class AudioLevelManagerDisconnected : AudioLevelManager
    {
        public AudioLevelManagerDisconnected() : base() 
        {
            SetValue(AudioLevelManager.IsAttachedProperty, true);
        }
    }

    abstract class AudioLevelManager : DependencyObject, IDisposable
    {

        public enum DeviceType
        {
            Unknown = 0,
            In,
            Out
        };

        public DeviceProvider Provider { get; protected set; }
        public DeviceType DeviceDirection { get; private set; }

        protected UnsignedMixerControl waveVolumeControl;

        private readonly MMDevice mmdevice;
        private readonly AudioEndpointVolume mmDeviceVolume;

        public static DependencyProperty IsAttachedProperty = DependencyProperty.Register("IsAttached", typeof(bool), typeof(AudioLevelManager), new PropertyMetadata(false));

        public static DependencyProperty MinProperty = DependencyProperty.Register("Min", typeof(double), typeof(AudioLevelManager), new PropertyMetadata((double)0, OnLevelChanged));
        public static DependencyProperty MaxProperty = DependencyProperty.Register("Max", typeof(double), typeof(AudioLevelManager), new PropertyMetadata((double)100, OnLevelChanged));
        public static DependencyProperty LevelProperty = DependencyProperty.Register("Level", typeof(double), typeof(AudioLevelManager), new PropertyMetadata((double)50, OnLevelChanged));
        public static DependencyProperty LevelPercentProperty = DependencyProperty.Register("LevelPercent", typeof(double), typeof(AudioLevelManager), new PropertyMetadata(50.0, OnLevelPercentChanged));

        protected AudioLevelManager()
        {
            Provider = DeviceProvider.Unknown;
            DeviceDirection = DeviceType.Unknown;
            Min = 0.0;
            Level = Max = 1.0;
        }

        public double Min
        {
            get { return (double)this.GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }
        public double Max
        {
            get { return (double)this.GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }
        public double Level
        {
            get { return (double)this.GetValue(LevelProperty); }
            set { SetValue(LevelProperty, value); }
        }

        double _LevelScalar;
        public double LevelScalar
        {
            get { return _LevelScalar; }
            set
            {
                _LevelScalar = value;
                SetValue(LevelPercentProperty, value * 100.0);
            }
        }

        private static void OnLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = d as AudioLevelManager;
            if (target != null)
            {
                if (!target.SuppressLevelEvent)
                    target.HandleLevelChange((double)(e.NewValue));
            }
        }

        private static void OnLevelPercentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = d as AudioLevelManager;
            if (target != null)
            {
                if (!target.SuppressLevelEvent)
                {
                    target.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        double newValue = ((double)e.NewValue) / 100.0;
                        double delta = target.Max - target.Min;
                        double offset = (delta * newValue);
                        target.Level = target.Min + offset;
                    }));
                }
            }
        }

        private void HandleLevelChange(double newLevel)
        {
            if (newLevel >= Min && newLevel <= Max)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    switch (Provider)
                    {
                        case DeviceProvider.Wave:
                            {
                                if (waveVolumeControl != null)
                                    try
                                    {
                                        waveVolumeControl.Value = (uint)newLevel;
                                    }
                                    catch (NAudio.MmException ex)
                                    {
                                        Console.WriteLine("Disabling Audio Controls: {0}", ex.Message);
                                        SetValue(IsAttachedProperty, false);
                                    }
                                break;
                            }
                        case DeviceProvider.Wasapi:
                            {
                                if (mmDeviceVolume != null)
                                    mmDeviceVolume.MasterVolumeLevel = (float)newLevel;
                                break;
                            }

                    }
                    UpdatePercent();
                }));
            }
        }

        private void UpdatePercent()
        {
            double delta = Max - Min;
            double offset = Level - Min;
            LevelScalar = offset / delta;
        }

        protected AudioLevelManager(WaveIn waveDevice) : this(GetVolumeMixerControlForInputLine(waveDevice.GetMixerLine())) { }
        protected AudioLevelManager(WaveInEvent waveDevice) : this(GetVolumeMixerControlForInputLine(waveDevice.GetMixerLine())) { }
        protected AudioLevelManager(UnsignedMixerControl waveVolumeControl)
        {
            Provider = DeviceProvider.Wave;
            DeviceDirection = DeviceType.In;

            this.waveVolumeControl = waveVolumeControl;

            SetValue(IsAttachedProperty, waveVolumeControl != null);

            if (waveVolumeControl != null)
            {
                Min = waveVolumeControl.MinValue;
                Max = waveVolumeControl.MaxValue;
                Level = waveVolumeControl.Value;
            }
        }

        protected AudioLevelManager(MMDevice mmdevice, DeviceType direction)
        {
            Provider = DeviceProvider.Wasapi;

            DeviceDirection = direction;

            this.mmdevice = mmdevice;

            mmDeviceVolume = mmdevice.AudioEndpointVolume;
            SetValue(IsAttachedProperty, mmDeviceVolume != null);

            if (mmDeviceVolume != null)
            {
                mmDeviceVolume.OnVolumeNotification += mmDeviceVolume_OnVolumeNotification;

                Min = mmDeviceVolume.VolumeRange.MinDecibels;
                Max = mmDeviceVolume.VolumeRange.MaxDecibels;
                Level = mmDeviceVolume.MasterVolumeLevel;
            }

        }

        private volatile bool SuppressLevelEvent = false;
        private object SuppressLevelLock = new object();
        void mmDeviceVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            Dispatcher.BeginInvoke(new Action<float>((level) =>
            {
                lock (SuppressLevelLock)
                {
                    SuppressLevelEvent = true;
                    LevelScalar = level;
                    SuppressLevelEvent = false;
                }
            }), data.MasterVolume);
        }

        protected bool isDisposed { get; private set; }
        public virtual void Dispose()
        {
            if (!isDisposed)
            {
                SetValue(IsAttachedProperty, false);
                isDisposed = true;
            }
        }


        protected static UnsignedMixerControl GetVolumeMixerControlForInputLine(MixerLine destination)
        {
            if (destination.ComponentType == MixerLineComponentType.DestinationWaveIn)
                foreach (MixerLine source in destination.Sources)
                {
                    if (source.ComponentType == MixerLineComponentType.SourceMicrophone)
                    {
                        foreach (MixerControl control in source.Controls)
                        {
                            if (control.ControlType == MixerControlType.Volume)
                                return (UnsignedMixerControl)control;
                        }
                    }
                }
            return null;
        }

        protected static UnsignedMixerControl GetVolumeMixerControlForOutputLine(Mixer mixer)
        {
            foreach (var mixerline in mixer.Destinations)
            {
                if (mixerline.ComponentType == MixerLineComponentType.DestinationSpeakers ||
                    mixerline.ComponentType == MixerLineComponentType.DestinationDigital ||
                    mixerline.ComponentType == MixerLineComponentType.DestinationHeadphones
                    )
                    foreach (MixerLine source in mixerline.Sources)
                    {
                        if (source.ComponentType == MixerLineComponentType.SourceWaveOut)
                        {
                            foreach (MixerControl control in source.Controls)
                            {
                                if (control.ControlType == MixerControlType.Volume)
                                    return (UnsignedMixerControl)control;
                            }
                        }
                    }
            }
            return null;
        }

    }
}
