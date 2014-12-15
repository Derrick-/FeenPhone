using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone
{
    public static class AudioExtensions
    {
        public static string Name(this FeenPhone.Audio.Codecs.INetworkChatCodec codec)
        {
            string bitRate = codec.BitsPerSecond == -1 ? "VBR" : String.Format("{0:0.#}kbps", codec.BitsPerSecond / 1000.0);
            string text = String.Format("{0} ({1})", codec.Name, bitRate);
            return text;
        }
    }
}

public class WaveOutCapabilitiesView
{
    public WaveOutCapabilities WaveOutCapabilities { get; private set; }
    public WaveOutCapabilitiesView(WaveOutCapabilities waveoutcapabilities)
    {
        WaveOutCapabilities = waveoutcapabilities;
    }

    public override string ToString()
    {
        return WaveOutCapabilities.ProductName ?? "Unknown";
    }

    public static implicit operator WaveOutCapabilities(WaveOutCapabilitiesView view)
    {
        return view.WaveOutCapabilities;
    }

    public static implicit operator WaveOutCapabilitiesView(WaveOutCapabilities struc)
    {
        return new WaveOutCapabilitiesView(struc);
    }
}
