using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone.Server.TcpPacketServer
{

    class ServerPacketHandler : BasePacketHandler
    {
        private readonly TcpPacketNetState state;

        public ServerPacketHandler(TcpPacketNetState state)
        {
            this.state = state;
        }

        protected override void OnChat(IEnumerable<byte> payload)
        {
            string text = Encoding.ASCII.GetString(payload.ToArray());
            FeenPhone.Server.EventSink.OnChat(state, text);
        }

    }
}
