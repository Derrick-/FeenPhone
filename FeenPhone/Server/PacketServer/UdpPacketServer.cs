using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alienseed.BaseNetworkServer.PacketServer;
using FeenPhone.Server.PacketServer;
using System.Net;

namespace FeenPhone.Server.PacketServer
{
    class UdpPacketServer : BaseUDPPacketServer<UdpPacketNetState>
    {
        const int readerBufferSize = ushort.MaxValue;

        public UdpPacketServer(int port = ServerHost.DefaultServerPort) : base(port) { }

        protected override UdpPacketNetState NetstateFactory(EndPoint ep)
        {
            return new UdpPacketNetState(ep as IPEndPoint, readerBufferSize);
        }

        protected override void ClientConnected(UdpPacketNetState state)
        {
            EventSink.OnConnect(state);
        }

        protected override void ClientDisconnected(UdpPacketNetState state)
        {
            EventSink.OnDisconnect(state);
        }

        protected override void ClientLogin(UdpPacketNetState state)
        {
            EventSink.OnLogin(state);
        }

        protected override void ClientLogout(UdpPacketNetState state)
        {
            EventSink.OnLogout(state);
        }
    }
}
