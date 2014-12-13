using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone.Client
{
    class ClientPacketHandler : BasePacketHandler
    {
        protected override void OnChat(IEnumerable<byte> payload)
        {
            string text = Encoding.ASCII.GetString(payload.ToArray());
            EventSource.InvokeOnChat(this, null, text);
        }
    }
}

