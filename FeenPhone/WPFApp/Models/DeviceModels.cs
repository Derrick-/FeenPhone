using FeenPhone.Audio;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Linq;

namespace FeenPhone.WPFApp.Models
{
    public enum DeviceProvider
    {
        Unknown = 0,
        Wave = 1,
        DirectSound = 2,
        Wasapi = 3,
    }

    public class InputDeviceModel : DeviceModel
    {
        public InputDeviceModel(int n, WaveInCapabilities capabilities) : base(n, capabilities) { }

        public InputDeviceModel(NAudio.CoreAudioApi.MMDevice device) : base(device) { }

        public int? LastLatency { get; set; }
    }

    public class OutputDeviceModel : DeviceModel
    {
        public OutputDeviceModel(DirectSoundDeviceInfo device) : base(device) { }

        public OutputDeviceModel(int n, NAudio.Wave.WaveOutCapabilities capabilities) : base(n, capabilities) { }

        public OutputDeviceModel(NAudio.CoreAudioApi.MMDevice device) : base(device) { }
    }

    public class DeviceModel
    {
        public DeviceProvider Provider { get; private set; }

        public MMDevice MMDevice { get; private set; }

        public DirectSoundDeviceInfo DirectSoundDeviceInfo { get; private set; }

        protected readonly WaveInCapabilities? WaveInCapabilities = null;
        protected readonly WaveOutCapabilities? WaveOutCapabilities = null;

        public bool IsWaveDevice { get { return WaveOutCapabilities != null || WaveInCapabilities != null; } }

        public int WavDeviceNumber { get; private set; }

        public DeviceModel(int wavDeviceNum, WaveInCapabilities waveincapabilities)
        {
            Provider = DeviceProvider.Wave;
            WavDeviceNumber = wavDeviceNum;
            WaveInCapabilities = waveincapabilities;
        }

        protected DeviceModel(int wavDeviceNum, WaveOutCapabilities waveoutcapabilities)
        {
            Provider = DeviceProvider.Wave;
            WavDeviceNumber = wavDeviceNum;
            WaveOutCapabilities = waveoutcapabilities;
        }

        public DeviceModel(DirectSoundDeviceInfo device)
        {
            DirectSoundDeviceInfo = device;
            Provider = DeviceProvider.DirectSound;
        }

        public DeviceModel(MMDevice device)
        {
            this.MMDevice = device;
            Provider = DeviceProvider.Wasapi;
        }

        Guid? _Guid = null;
        public Guid Guid
        {
            get
            {
                if (!_Guid.HasValue)
                {
                    if (Provider == DeviceProvider.Wave)
                        return (_Guid ?? (_Guid = (WaveOutCapabilities.HasValue ? WaveOutCapabilities.Value.ProductGuid : WaveInCapabilities.Value.ProductGuid)).Value);

                    else if (Provider == DeviceProvider.DirectSound)
                        return (_Guid ?? (_Guid = DirectSoundDeviceInfo.Guid).Value);

                    else if (Provider == DeviceProvider.Wasapi)
                        return (_Guid ?? (_Guid = MMDevices.GetWasapiGuid(MMDevice)).Value);

                    else
                        _Guid = Guid.Empty;
                }

                return _Guid.Value;
            }
        }

        public string Name
        {
            get { return this.ToString(); }
        }

        public override string ToString()
        {
            if (Provider == DeviceProvider.Wave)
                return "WAV: " + ((WaveOutCapabilities.HasValue ? WaveOutCapabilities.Value.ProductName : WaveInCapabilities.Value.ProductName) ?? "Unknown");

            if (Provider == DeviceProvider.DirectSound)
                return "DSO: " + (DirectSoundDeviceInfo.Description ?? "Unknown");

            if (Provider == DeviceProvider.Wasapi)
                return "WASAPI: " + (MMDevice.FriendlyName ?? "Unknown");

            return "Unknown";
        }
    }

}