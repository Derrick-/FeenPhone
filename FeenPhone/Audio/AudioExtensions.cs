using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone
{
    static class AudioExtensions
    {
        public static string ToString(this FeenPhone.Audio.Codecs.INetworkChatCodec codec)
        {
            string bitRate = codec.BitsPerSecond == -1 ? "VBR" : String.Format("{0:0.#}kbps", codec.BitsPerSecond / 1000.0);
            string text = String.Format("{0} ({1})", codec.Name, bitRate);
            return text;
        }
    }
}
