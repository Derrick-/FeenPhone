using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alienseed.BaseNetworkServer.PacketServer;
using FeenPhone.Server.TcpPacketServer;
using System.Net;

namespace FeenPhone.Server.TcpPacketServer
{
    class UdpPacketServer : BaseUDPPacketServer<UdpPacketNetState>
    {
        const int readerBufferSize = ushort.MaxValue;

        public UdpPacketServer(int port = ServerHost.DefaultServerPort) : base(port) { }

        protected override UdpPacketNetState NetstateFactory(EndPoint ep)
        {
            return new UdpPacketNetState(ep as IPEndPoint, readerBufferSize);
        }

        protected override void ClientConnected(UdpPacketNetState client)
        {
            EventSink.OnConnect(client);
        }

        protected override void ClientDisconnected(UdpPacketNetState client)
        {
            EventSink.OnDisconnect(client);
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
