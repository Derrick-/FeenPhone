using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;
using System.ComponentModel.Composition;

namespace FeenPhone.Audio.Codecs
{
    [Export(typeof(INetworkChatCodec))]
    class UncompressedMonoPcmChatCodec : BaseUncompressedPcmChatCodec
    {
        public override CodecID CodecID { get { return CodecID.Uncompressed44KhzMonoPcmChatCodec; } }

        public override bool IsAvailable { get { return true; } }
        public UncompressedMonoPcmChatCodec() : base(1, 44100) { }
    }

    [Export(typeof(INetworkChatCodec))]
    class UncompressedStereoPcmChatCodec : BaseUncompressedPcmChatCodec
    {
        public override CodecID CodecID { get { return CodecID.Uncompressed44KHzStereoPcmChatCodec; } }

        public override bool IsAvailable { get { return true; } }
        public UncompressedStereoPcmChatCodec() : base(2, 44100) { }
    }

    [Export(typeof(INetworkChatCodec))]
    class Uncompressed8KHzMonoPcmChatCodec : BaseUncompressedPcmChatCodec
    {
        public override CodecID CodecID { get { return CodecID.Uncompressed8KHzMonoPcmChatCodec; } }

        public override bool IsAvailable { get { return true; } }
        public Uncompressed8KHzMonoPcmChatCodec() : base(1, 8000) { }
    }

    [Export(typeof(INetworkChatCodec))]
    class Uncompressed8KHzStereoPcmChatCodec : BaseUncompressedPcmChatCodec
    {
        public override CodecID CodecID { get { return CodecID.Uncompressed8KHzStereoPcmChatCodec; } }

        public override bool IsAvailable { get { return true; } }
        public Uncompressed8KHzStereoPcmChatCodec() : base(2, 8000) { }
    }

    abstract class BaseUncompressedPcmChatCodec : INetworkChatCodec
    {
        abstract public bool IsAvailable { get; }

        int Channels;
        int Freq;

        public BaseUncompressedPcmChatCodec(int channels, int rate)
        {
            this.Channels = channels;
            this.Freq = rate;

            this.RecordFormat = new WaveFormat(Freq, 16, Channels);
        }

        public abstract CodecID CodecID { get; }

        public string Name { get { return "PCM " + Freq / 1000 + "kHz 16 bit " + Channels + "ch uncompressed"; } }

        public WaveFormat RecordFormat { get; private set; }

        public byte[] Encode(byte[] data, int length)
        {
            return Encode(data, 0, length);
        }

        public byte[] Encode(byte[] data, int offset, int length)
        {
            byte[] encoded = new byte[length];
            Array.Copy(data, offset, encoded, 0, length);
            return encoded;
        }

        public byte[] Decode(byte[] data, int length)
        {
            return Decode(data, 0, length);
        }

        public byte[] Decode(byte[] data, int offset, int length)
        {
            byte[] decoded = new byte[length];
            Array.Copy(data, offset, decoded, 0, length);
            return decoded;
        }

        public int BitsPerSecond { get { return this.RecordFormat.AverageBytesPerSecond * 8; } }

        public void Dispose() { }
    }
}
