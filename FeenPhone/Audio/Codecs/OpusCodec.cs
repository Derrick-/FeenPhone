using FragLabs.Audio.Codecs;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeenPhone.Audio.Codecs
{
    [Export(typeof(INetworkChatCodec))]
    class OpusCodec24kHzVoip8192 : OpusCodec
    {
        public override CodecID CodecID { get { return CodecID.OpusCodec24kHzVoip8192; } }
        public override bool IsAvailable { get { return false; } }

        public OpusCodec24kHzVoip8192() : base(bitRate: 8192, outputSampleRate: 24000, opusMode: FragLabs.Audio.Codecs.Opus.Application.Voip) { }
    }

    [Export(typeof(INetworkChatCodec))]
    class OpusCodec24kHzVoip16384 : OpusCodec
    {
        public override CodecID CodecID { get { return CodecID.OpusCodec24kHzVoip16384; } }
        public override bool IsAvailable { get { return base.IsAvailable; } }

        public OpusCodec24kHzVoip16384() : base(bitRate: 16384, outputSampleRate: 24000, opusMode: FragLabs.Audio.Codecs.Opus.Application.Voip) { }
    }

    [Export(typeof(INetworkChatCodec))]
    class OpusCodec24kHzVoip32768 : OpusCodec
    {
        public override CodecID CodecID { get { return CodecID.OpusCodec24kHzVoip32768; } }
        public override bool IsAvailable { get { return base.IsAvailable; } }

        public OpusCodec24kHzVoip32768() : base(bitRate: 32768, outputSampleRate: 24000, opusMode: FragLabs.Audio.Codecs.Opus.Application.Voip) { }
    }

    [Export(typeof(INetworkChatCodec))]
    class OpusCodecAudio24kHz8192 : OpusCodec
    {
        public override CodecID CodecID { get { return CodecID.OpusCodecAudio24kHz8192; } }
        public override bool IsAvailable { get { return false; } }

        public OpusCodecAudio24kHz8192() : base(bitRate: 8192, outputSampleRate: 24000, opusMode: FragLabs.Audio.Codecs.Opus.Application.Audio) { }
    }

    [Export(typeof(INetworkChatCodec))]
    class OpusCodecAudio24kHz16384 : OpusCodec
    {
        public override CodecID CodecID { get { return CodecID.OpusCodecAudio24kHz16384; } }
        public override bool IsAvailable { get { return false; } }

        public OpusCodecAudio24kHz16384() : base(bitRate: 16384, outputSampleRate: 24000, opusMode: FragLabs.Audio.Codecs.Opus.Application.Audio) { }
    }

    [Export(typeof(INetworkChatCodec))]
    class OpusCodecAudio24kHz32768 : OpusCodec
    {
        public override CodecID CodecID { get { return CodecID.OpusCodecAudio24kHz32768; } }
        public override bool IsAvailable { get { return false; } }

        public OpusCodecAudio24kHz32768() : base(bitRate: 32768, outputSampleRate: 24000, opusMode: FragLabs.Audio.Codecs.Opus.Application.Audio) { }
    }

    [Export(typeof(INetworkChatCodec))]
    class OpusCodecAudio48kHz32768 : OpusCodec
    {
        public override CodecID CodecID { get { return CodecID.OpusCodecAudio48kHz65536; } }
        public override bool IsAvailable { get { return base.IsAvailable; } }

        public OpusCodecAudio48kHz32768() : base(bitRate: 65536, outputSampleRate: 48000, opusMode: FragLabs.Audio.Codecs.Opus.Application.Audio) { }
    }

    [Export(typeof(INetworkChatCodec))]
    class OpusCodec24kHzLowLatency8192 : OpusCodec
    {
        public override CodecID CodecID { get { return CodecID.OpusCodec24kHzLowLatency8192; } }
        public override bool IsAvailable { get { return false; } }
 
        public OpusCodec24kHzLowLatency8192() : base(bitRate: 8192, outputSampleRate: 24000, opusMode: FragLabs.Audio.Codecs.Opus.Application.Restricted_LowLatency) { }
    }

    [Export(typeof(INetworkChatCodec))]
    class OpusCodec24kHzLowLatency16384 : OpusCodec
    {
        public override CodecID CodecID { get { return CodecID.OpusCodec24kHzLowLatency16384; } }
        public override bool IsAvailable { get { return false; } }

        public OpusCodec24kHzLowLatency16384() : base(bitRate: 16384, outputSampleRate: 24000, opusMode: FragLabs.Audio.Codecs.Opus.Application.Restricted_LowLatency) { }
    }

    [Export(typeof(INetworkChatCodec))]
    class OpusCodec24kHzLowLatency32768 : OpusCodec
    {
        public OpusCodec24kHzLowLatency32768() : base(bitRate: 32768, outputSampleRate: 24000, opusMode: FragLabs.Audio.Codecs.Opus.Application.Restricted_LowLatency) { }
        public override bool IsAvailable { get { return false; } }

        public override CodecID CodecID { get { return CodecID.OpusCodec24kHzLowLatency32768; } }
    }

    abstract class OpusCodec : INetworkChatCodec
    {
        public abstract CodecID CodecID { get; }

        int outputSampleRate;
        int bitRate;
        int channels = 1;

        NAudio.Wave.WaveFormat _recordFormat;

        FragLabs.Audio.Codecs.Opus.Application opusMode;

        public OpusCodec(int bitRate, int outputSampleRate, FragLabs.Audio.Codecs.Opus.Application opusMode)
        {
            this.outputSampleRate = outputSampleRate;
            this.bitRate = bitRate;
            this.opusMode = opusMode;

            _segmentFrames = 960;

            _recordFormat = new WaveFormat(outputSampleRate, 16 * channels, channels);

            CreateEncoder();
            CreateDecoder();

        }

        private OpusEncoder _Encoder = null;
        OpusEncoder Encoder
        {
            get
            {
                if (_Encoder == null)
                    CreateEncoder();

                return _Encoder;
            }
        }

        private OpusDecoder _Decoder = null;
        OpusDecoder Decoder
        {
            get 
            {
                if (_Decoder == null)
                    CreateDecoder();

                return _Decoder; 
            }
        }

        private void CreateEncoder()
        {
            _Encoder = OpusEncoder.Create(outputSampleRate, channels, opusMode);
            _Encoder.Bitrate = bitRate * channels;
            _bytesPerSegment = _Encoder.FrameByteCount(_segmentFrames);
        }

        private void CreateDecoder()
        {
            _Decoder = OpusDecoder.Create(outputSampleRate, channels);
        }

        int _segmentFrames;
        int _bytesPerSegment;

        public virtual string Name
        {
            get
            {
                string mode="Unknown";
                switch(opusMode)
                {
                    case FragLabs.Audio.Codecs.Opus.Application.Audio:
                        mode="Music"; break;
                    case FragLabs.Audio.Codecs.Opus.Application.Restricted_LowLatency:
                        mode="LowLatency"; break;
                    case FragLabs.Audio.Codecs.Opus.Application.Voip:
                        mode="Talk"; break;
                }
                return string.Format("Opus {0} {1}kHz", mode, outputSampleRate / 1000);
            }
        }

        public virtual bool IsAvailable { get { return true; } }
        public virtual int SortOrder { get { return 10; } }

        public int BitsPerSecond
        {
            get { return bitRate; }
        }

        public NAudio.Wave.WaveFormat RecordFormat
        {
            get { return _recordFormat; }
        }

        byte[] _notEncodedBuffer = new byte[0];
        public byte[] Encode(byte[] data, int length)
        {
            //Stopwatch watch = new Stopwatch();
            //watch.Start();

            byte[] soundBuffer = new byte[length + _notEncodedBuffer.Length];
            for (int i = 0; i < _notEncodedBuffer.Length; i++)
                soundBuffer[i] = _notEncodedBuffer[i];
            for (int i = 0; i < length; i++)
                soundBuffer[i + _notEncodedBuffer.Length] = data[i];

            int byteCap = _bytesPerSegment;
            int segmentCount = (int)Math.Floor((decimal)soundBuffer.Length / byteCap);
            int segmentsEnd = segmentCount * byteCap;
            int notEncodedCount = soundBuffer.Length - segmentsEnd;
            _notEncodedBuffer = new byte[notEncodedCount];
            for (int i = 0; i < notEncodedCount; i++)
            {
                _notEncodedBuffer[i] = soundBuffer[segmentsEnd + i];
            }

            ulong encoded = 0;

            List<OpusFrame> frames = new List<OpusFrame>();

            for (int i = 0; i < segmentCount; i++)
            {
                byte[] segment = new byte[byteCap];
                for (int j = 0; j < segment.Length; j++)
                    segment[j] = soundBuffer[(i * byteCap) + j];
                int len;
                byte[] buff = Encoder.Encode(segment, segment.Length, out len);
                encoded += (ulong)len;

                frames.Add(new OpusFrame(buff, (byte)len));

                //Debug.WriteLine(String.Format("Opus: In {0} bytes, encoded {1} bytes [enc frame size = {2}]", segment.Length, len, _bytesPerSegment));
            }


            var result = frames.SelectMany(m => m.GetPacket()).ToArray();

            //watch.Stop();
            //Debug.WriteLine("Encode time: {0}ms", watch.ElapsedMilliseconds);

            return result;
        }

        private class OpusFrame
        {
            byte len;
            byte[] data;

            public byte[] Data
            {
                get { return data; }
            }
            public byte Len
            {
                get { return len; }
            }

            public static IEnumerable<OpusFrame> ReadPackets(byte[] data, int length)
            {
                List<OpusFrame> list = new List<OpusFrame>();
                if (length > 0)
                {
                    byte len = data[0];
                    int pos = 1;
                    while (pos < Math.Min(length, data.Length))
                    {
                        list.Add(new OpusFrame(data.Skip(pos).Take(len).ToArray(), len));
                        pos += len;
                        if (pos >= length) break;
                        len = data[pos++];
                    }
                }
                return list;
            }

            public OpusFrame(byte[] data, byte len)
            {
                this.data = data;
                this.len = len;
            }

            byte[] _packet = null;
            public byte[] GetPacket()
            {
                if (_packet == null)
                {
                    _packet = new byte[len + 1];
                    _packet[0] = len;
                    for (int i = 0; i < len; i++)
                        _packet[i + 1] = data[i];
                }
                return _packet;
            }
        }

        public byte[] Decode(byte[] data, int length)
        {
            //Stopwatch watch = new Stopwatch();
            //watch.Start();

            List<IEnumerable<byte>> wavData = new List<IEnumerable<byte>>();
            var frames = OpusFrame.ReadPackets(data, length);

            foreach (var frame in frames)
            {
                int len;
                byte[] buff = Decoder.Decode(frame.Data, frame.Len, out len);

                //Debug.WriteLine(String.Format("Opus: In {0} bytes, decoded {1} bytes [dec frame size = {2}]", frame.Len, len, _bytesPerSegment));

                wavData.Add(buff.Take(len));
            }

            var result = wavData.SelectMany(m => m).ToArray();

            //watch.Stop();
            //Debug.WriteLine("Decode time: {0}ms", watch.ElapsedMilliseconds);

            return result;
        }

        public void Dispose()
        {
            var e = Encoder;
            var d = _Decoder;

            _Encoder = null;
            _Decoder = null;

            if (e != null)
                e.Dispose();
            if (d != null)
                d.Dispose();

        }
    }
}
