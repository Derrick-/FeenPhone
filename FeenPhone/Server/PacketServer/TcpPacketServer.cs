using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alienseed.BaseNetworkServer.PacketServer;
using FeenPhone.Server.PacketServer;
using System.Net;

namespace FeenPhone.Server.PacketServer
{
    class TcpPacketServer : BaseTCPPacketServer<TcpPacketNetState>
    {
        const int readerBufferSize = ushort.MaxValue;

        public TcpPacketServer(int port = ServerHost.DefaultServerPort, IPAddress ip = null, bool noDelay = false) : base(port, ip, noDelay) { }

        protected override TcpPacketNetState NetstateFactory(System.Net.Sockets.NetworkStream stream, System.Net.EndPoint ep)
        {
            return new TcpPacketNetState(stream, ep as IPEndPoint, readerBufferSize);
        }

        protected override void ClientConnected(TcpPacketNetState state)
        {
            EventSink.OnConnect(state);
        }

        protected override void ClientDisconnected(TcpPacketNetState state)
        {
            EventSink.OnDisconnect(state);
        }

        protected override void ClientLogin(TcpPacketNetState state)
        {
            EventSink.OnLogin(state);
        }

        protected override void ClientLogout(TcpPacketNetState state)
        {
            EventSink.OnLogout(state);
        }
    }
}
