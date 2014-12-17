using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alienseed.BaseNetworkServer.PacketServer;
using FeenPhone.Server.TcpPacketServer;
using System.Net;

namespace FeenPhone.Server.TcpPacketServer
{
    class TcpPacketServer : BaseTCPPacketServer<TcpPacketNetState>
    {
        const int readerBufferSize = ushort.MaxValue;

        public const int DefaultTcpServerPort = 5150;

        public TcpPacketServer(int port = DefaultTcpServerPort, IPAddress ip = null) : base(port, ip) { }

        protected override TcpPacketNetState NetstateFactory(System.Net.Sockets.NetworkStream stream, System.Net.EndPoint ep)
        {
            return new TcpPacketNetState(stream, ep as IPEndPoint, readerBufferSize);
        }

        protected override void ClientConnected(TcpPacketNetState client)
        {
            Packet.WriteLoginStatus(client.Writer, false);
            EventSink.OnConnect(client);
        }

        protected override void ClientDisconnected(TcpPacketNetState client)
        {
            EventSink.OnDisconnect(client);
        }

        protected override void ClientLogin(Alienseed.BaseNetworkServer.Accounting.IUserClient client)
        {
            EventSink.OnLogin(client);
        }

        protected override void ClientLogout(Alienseed.BaseNetworkServer.Accounting.IUserClient client)
        {
            EventSink.OnLogout(client);
        }
    }
}
