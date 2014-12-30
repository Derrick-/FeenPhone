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
        private WaveIn waveDevice;
        private UnsignedMixerControl waveVolumeControl;

        private WasapiCapture waspiDevice;

        public static DependencyProperty IsAttachedProperty = DependencyProperty.Register("IsAttached", typeof(bool), typeof(InputLevelManager), new PropertyMetadata(false));

        public static DependencyProperty MinProperty = DependencyProperty.Register("Min", typeof(uint), typeof(InputLevelManager), new PropertyMetadata((uint)0));
        public static DependencyProperty MaxProperty = DependencyProperty.Register("Max", typeof(uint), typeof(InputLevelManager), new PropertyMetadata((uint)100));
        public static DependencyProperty LevelProperty = DependencyProperty.Register("Level", typeof(uint), typeof(InputLevelManager), new PropertyMetadata((uint)50, OnLevelChanged));

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

        public uint Min
        {
            get { return (uint)this.GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }
        public uint Max
        {
            get { return (uint)this.GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }
        public uint Level
        {
            get { return (uint)this.GetValue(LevelProperty); }
            set { SetValue(LevelProperty, value); }
        }

        private static void OnLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = d as InputLevelManager;
            if (target != null)
            {
                target.HandleLevelChange((uint)(e.NewValue));
            }
        }

        private void HandleLevelChange(uint newLevel)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                switch (Provider)
                {
                    case DeviceProvider.Wave:
                        {
                            if (waveVolumeControl != null)
                                waveVolumeControl.Value = newLevel;
                            break;
                        }
                }
            }));
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

        public InputLevelManager(WasapiCapture waspiDevice)
        {
            Provider = DeviceProvider.Wasapi;
            this.waspiDevice = waspiDevice;
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
