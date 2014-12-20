using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace FeenPhone.Audio.Codecs
{
    public enum CodecID : byte
    {   // Changing these will break backwards compatibility
        OpusCodec24kHzLowLatency32768 = 10,
        OpusCodec24kHzVoip8192 = 21,
        OpusCodec24kHzVoip16384 = 32,
        OpusCodec24kHzVoip32768 = 43,
        OpusCodecAudio24kHz8192 = 54,
        OpusCodecAudio24kHz16384 = 65,
        OpusCodecAudio24kHz32768 = 76,
        OpusCodec24kHzLowLatency8192 = 87,
        OpusCodec24kHzLowLatency16384 = 98,
        Uncompressed44KhzMonoPcmChatCodec = 100,
        Uncompressed8KHzMonoPcmChatCodec = 101,
        Uncompressed44KHzStereoPcmChatCodec = 105,
        Uncompressed8KHzStereoPcmChatCodec = 106,
        AcmMuLawChatCodec = 110,
        ALawChatCodec = 120,
        AcmALawChatCodec = 130,
        G722ChatCodec = 140,
        Gsm610ChatCodec = 150,
        MicrosoftAdpcmChatCodec = 160,
        MuLawChatCodec = 170,
        NarrowBandSpeexCodec = 180,
        WideBandSpeexCodec = 190,
        UltraWideBandSpeexCodec = 200,
        TrueSpeechChatCodec = 210,
    }

    public interface INetworkChatCodec : IDisposable
    {

        CodecID CodecID { get; }

        /// <summary>
        /// Friendly Name for this codec
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Tests whether the codec is available on this system
        /// </summary>
        bool IsAvailable { get; }
        /// <summary>
        /// Bitrate
        /// </summary>
        int BitsPerSecond { get; }
        /// <summary>
        /// Preferred PCM format for recording in (usually 8kHz mono 16 bit)
        /// </summary>
        WaveFormat RecordFormat { get; }
        /// <summary>
        /// Encodes a block of audio
        /// </summary>
        byte[] Encode(byte[] data, int length);
        /// <summary>
        /// Decodes a block of audio
        /// </summary>
        byte[] Decode(byte[] data, int length);
    }
}
