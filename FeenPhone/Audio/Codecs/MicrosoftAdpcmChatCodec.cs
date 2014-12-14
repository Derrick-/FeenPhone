using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;
using System.ComponentModel.Composition;

namespace FeenPhone.Audio.Codecs
{
    [Export(typeof(INetworkChatCodec))]
    class MicrosoftAdpcmChatCodec : AcmChatCodec
    {
        public override CodecID CodecID { get { return CodecID.MicrosoftAdpcmChatCodec; } }

        public MicrosoftAdpcmChatCodec()
            : base(new WaveFormat(8000, 16, 1), new AdpcmWaveFormat(8000,1))
        {
        }

        public override string Name { get { return "Microsoft ADPCM"; } }
    }
}
