using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeenPhone.WPFApp.Models
{
    public class InputDeviceModel
    {
        public enum InputDeviceProvider
        {
            Unknown = 0,
            Wave = 1,
            Wasapi = 2,
        }

        public InputDeviceProvider Provider { get; private set; }

        public WaveInCapabilities WaveInCapabilities { get; private set; }
        public int WavDeviceNumber { get; private set; }

        public NAudio.CoreAudioApi.MMDevice MMDevice { get; private set; }

        public InputDeviceModel(int wavDeviceNum, WaveInCapabilities waveincapabilities)
        {
            Provider = InputDeviceProvider.Wave;
            WavDeviceNumber = wavDeviceNum;
            WaveInCapabilities = waveincapabilities;
        }

        public InputDeviceModel(MMDevice device)
        {
            Provider = InputDeviceProvider.Wasapi;
            this.MMDevice = device;
        }

        public override string ToString()
        {
            switch (Provider)
            {
                case InputDeviceProvider.Wave: return WaveInCapabilities.ProductName + " (WAVE)";
                case InputDeviceProvider.Wasapi: return MMDevice.FriendlyName + " (Wasapi)";
                default: return "Unknown";
            }
        }

    }
}
