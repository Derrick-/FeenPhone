using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;
using System.ComponentModel.Composition;

namespace FeenPhone.Audio.Codecs
{
    [Export(typeof(INetworkChatCodec))]
    class Gsm610ChatCodec : AcmChatCodec
    {
        public override CodecID CodecID { get { return CodecID.Gsm610ChatCodec; } }

        public Gsm610ChatCodec()
            : base(new WaveFormat(8000, 16, 1), new Gsm610WaveFormat())
        {
        }

        public override string Name { get { return "GSM 6.10"; } }
    }
}
