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
    class InputLevelManager : DependencyObject, IDisposable
    {
        public DeviceProvider Provider { get; private set; }

        private readonly WaveIn waveDevice;
        private readonly UnsignedMixerControl waveVolumeControl;

        private readonly WasapiCapture wasapi;
        private readonly MMDevice mmdevice;
        private readonly AudioEndpointVolume mmDeviceVolume;

        public static DependencyProperty IsAttachedProperty = DependencyProperty.Register("IsAttached", typeof(bool), typeof(InputLevelManager), new PropertyMetadata(false));

        public static DependencyProperty MinProperty = DependencyProperty.Register("Min", typeof(double), typeof(InputLevelManager), new PropertyMetadata((double)0, OnLevelChanged));
        public static DependencyProperty MaxProperty = DependencyProperty.Register("Max", typeof(double), typeof(InputLevelManager), new PropertyMetadata((double)100, OnLevelChanged));
        public static DependencyProperty LevelProperty = DependencyProperty.Register("Level", typeof(double), typeof(InputLevelManager), new PropertyMetadata((double)50, OnLevelChanged));
        public static DependencyProperty LevelPercentProperty = DependencyProperty.Register("LevelPercent", typeof(double), typeof(InputLevelManager), new PropertyMetadata(50.0, OnLevelPercentChanged));

        public InputLevelManager()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            var settings = Settings.Container;
            Level = settings.DefaultInputLevel;
        }

        private void SaveSettings()
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

        double _LevelPercent;
        public double LevelPercent
        {
            get { return _LevelPercent; }
            set
            {
                _LevelPercent = value;
                SetValue(LevelPercentProperty, value * 100.0);
            }
        }

        private static void OnLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = d as InputLevelManager;
            if (target != null)
            {
                target.HandleLevelChange((double)(e.NewValue));
            }
        }

        private static void OnLevelPercentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = d as InputLevelManager;
            if (target != null)
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

        private void HandleLevelChange(double newLevel)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                switch (Provider)
                {
                    case DeviceProvider.Wave:
                        {
                            if (waveVolumeControl != null)
                                waveVolumeControl.Value = (uint)newLevel;
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

        private void UpdatePercent()
        {
            double delta = Max - Min;
            double offset = Level - Min;
            LevelPercent = offset / delta;
        }

        public InputLevelManager(WaveIn waveDevice)
        {
            Provider = DeviceProvider.Wave;
            this.waveDevice = waveDevice;

            waveVolumeControl = GetVolumeMixerControlForLine(waveDevice.GetMixerLine());
            SetValue(IsAttachedProperty, waveVolumeControl != null);

            if (waveVolumeControl != null)
            {
                Min = waveVolumeControl.MinValue;
                Max = waveVolumeControl.MaxValue;
                Level = waveVolumeControl.Value;
            }
        }

        public InputLevelManager(WasapiCapture waspicapture, MMDevice mmdevice)
        {
            Provider = DeviceProvider.Wasapi;
            this.wasapi = waspicapture;
            this.mmdevice = mmdevice;

            mmDeviceVolume = mmdevice.AudioEndpointVolume;
            SetValue(IsAttachedProperty, mmDeviceVolume != null);

            if (mmDeviceVolume != null)
            {
                Min = mmDeviceVolume.VolumeRange.MinDecibels;
                Max = mmDeviceVolume.VolumeRange.MaxDecibels;
                Level = mmDeviceVolume.MasterVolumeLevel;
            }

        }

        bool isDisposed = false;
        public void Dispose()
        {
            if (!isDisposed)
            {
                SaveSettings();
                SetValue(IsAttachedProperty, false);
                isDisposed = true;
            }
        }


        public UnsignedMixerControl GetVolumeMixerControlForLine(MixerLine destination)
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
