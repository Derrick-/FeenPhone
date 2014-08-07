using Alienseed.BaseNetworkServer;
using Alienseed.BaseNetworkServer.Telnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace FeenPhone.Server.Telnet
{
    internal class TelnetServer : BaseTelnetServer<TelNetState>
    {
        protected override TelNetState NetstateFactory(System.Net.Sockets.NetworkStream stream, System.Net.EndPoint ep)
        {
            return new TelNetState(stream, ep as IPEndPoint);
        }

        protected override void ClientConnected(NetState client)
        {
            EventSink.OnConnect(client);
        }

        protected override void ClientDisconnected(NetState client)
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
