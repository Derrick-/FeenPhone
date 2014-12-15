using NAudio.Wave;
using System;

namespace FeenPhone.WPFApp.Models
{

    public class OutputDeviceModel
    {

        public enum OutputDeviceProvider
        {
            Unknown = 0,
            Wave = 1,
            DirectSound = 2,
        }

        public OutputDeviceProvider Provider { get; private set; }

        public DirectSoundDeviceInfo DirectSoundDeviceInfo { get; private set; }

        public WaveOutCapabilities WaveOutCapabilities { get; private set; }
        public int WavDeviceNumber { get; private set; }

        public OutputDeviceModel(int wavDeviceNum, WaveOutCapabilities waveoutcapabilities)
        {
            Provider = OutputDeviceProvider.Wave;
            WavDeviceNumber = wavDeviceNum;
            WaveOutCapabilities = waveoutcapabilities;
        }

        public OutputDeviceModel(DirectSoundDeviceInfo device)
        {
            if (device is DirectSoundDeviceInfo)
            {
                Provider = OutputDeviceProvider.DirectSound;
                DirectSoundDeviceInfo = device;
            }
            else
                Provider = OutputDeviceProvider.Unknown;
        }

        public Guid Guid
        {
            get
            {
                if (Provider == OutputDeviceProvider.Wave)
                    return WaveOutCapabilities.ProductGuid;

                if (Provider == OutputDeviceProvider.DirectSound)
                    return DirectSoundDeviceInfo.Guid;

                return Guid.Empty;
            }
        }

        public override string ToString()
        {
            if (Provider == OutputDeviceProvider.Wave)
                return "WAV: " + (WaveOutCapabilities.ProductName ?? "Unknown");

            if (Provider == OutputDeviceProvider.DirectSound)
                return "DSO: " + (DirectSoundDeviceInfo.Description ?? "Unknown");

            return "Unknown";
        }
    }

}