using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeenPhone.WPFApp.Models
{
    public class CodecModel
    {
        public string Text { get; set; }
        public Audio.Codecs.INetworkChatCodec Codec { get; set; }
        public override string ToString()
        {
            return Text;
        }
    }

}
