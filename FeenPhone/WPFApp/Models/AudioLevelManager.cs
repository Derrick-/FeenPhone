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
    class AudioLevelManager : DependencyObject, IDisposable
    {

        public enum DeviceType
        {
            Unknown = 0,
            In,
            Out
        };

        public DeviceProvider Provider { get; private set; }
        public DeviceType DeviceDiection { get; private set; }

        private readonly WaveIn waveInDevice;
        private readonly WaveOut waveOutDevice;
        private readonly UnsignedMixerControl waveVolumeControl;

        private readonly WasapiCapture wasapiInDevice;
        private readonly WasapiOut wasapiOutDevice;
        private readonly MMDevice mmdevice;
        private readonly AudioEndpointVolume mmDeviceVolume;

        public static DependencyProperty IsAttachedProperty = DependencyProperty.Register("IsAttached", typeof(bool), typeof(AudioLevelManager), new PropertyMetadata(false));

        public static DependencyProperty MinProperty = DependencyProperty.Register("Min", typeof(double), typeof(AudioLevelManager), new PropertyMetadata((double)0, OnLevelChanged));
        public static DependencyProperty MaxProperty = DependencyProperty.Register("Max", typeof(double), typeof(AudioLevelManager), new PropertyMetadata((double)100, OnLevelChanged));
        public static DependencyProperty LevelProperty = DependencyProperty.Register("Level", typeof(double), typeof(AudioLevelManager), new PropertyMetadata((double)50, OnLevelChanged));
        public static DependencyProperty LevelPercentProperty = DependencyProperty.Register("LevelPercent", typeof(double), typeof(AudioLevelManager), new PropertyMetadata(50.0, OnLevelPercentChanged));

        public AudioLevelManager()
        {
            Provider = DeviceProvider.Unknown;
            DeviceDiection = DeviceType.Unknown;
            Min = 0.0;
            Level = Max = 1.0;
        }

        private void LoadInputSettings()
        {
            var settings = Settings.Container;
            Level = settings.DefaultInputLevel;
        }

        private void SaveInputSettings()
        {
            var settings = Settings.Container;
            settings.DefaultInputLevel = Level;
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
                                if (DeviceDiection == DeviceType.In)
                                {
                                    if (waveVolumeControl != null)
                                        waveVolumeControl.Value = (uint)newLevel;
                                }
                                else
                                {
                                    waveOutDevice.Volume = (float)newLevel;
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

        public AudioLevelManager(WaveIn waveDevice)
        {
            LoadInputSettings();

            Provider = DeviceProvider.Wave;
            DeviceDiection = DeviceType.In;
            this.waveInDevice = waveDevice;

            waveVolumeControl = GetVolumeMixerControlForInputLine(waveDevice.GetMixerLine());
            SetValue(IsAttachedProperty, waveVolumeControl != null);

            if (waveVolumeControl != null)
            {
                Min = waveVolumeControl.MinValue;
                Max = waveVolumeControl.MaxValue;
                Level = waveVolumeControl.Value;
            }
        }

        public AudioLevelManager(WaveOut waveDevice)
        {
            Provider = DeviceProvider.Wave;
            DeviceDiection = DeviceType.Out;
            this.waveOutDevice = waveDevice;

            Min = 0.0;
            Max = 1.0;
            Level = waveDevice.Volume;
        }


        public AudioLevelManager(WasapiCapture waspicapture, MMDevice mmdevice)
            : this(mmdevice, DeviceType.In)
        {
            LoadInputSettings();

            this.wasapiInDevice = waspicapture;
        }

        public AudioLevelManager(WasapiOut waspiout, MMDevice mmdevice)
            : this(mmdevice, DeviceType.Out)
        {
            this.wasapiOutDevice = waspiout;
        }

        private AudioLevelManager(MMDevice mmdevice, DeviceType direction)
        {
            Provider = DeviceProvider.Wasapi;

            DeviceDiection = direction;

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

        bool isDisposed = false;
        public void Dispose()
        {
            if (!isDisposed)
            {
                if (DeviceDiection == DeviceType.In)
                    SaveInputSettings();

                SetValue(IsAttachedProperty, false);
                isDisposed = true;
            }
        }


        public UnsignedMixerControl GetVolumeMixerControlForInputLine(MixerLine destination)
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

        public UnsignedMixerControl GetVolumeMixerControlForOutputLine(MixerLine destination)
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

    }
}
