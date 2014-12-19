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

        protected override void ClientConnected(TelNetState state)
        {
            EventSink.OnConnect(state);
        }

        protected override void ClientDisconnected(TelNetState state)
        {
            EventSink.OnDisconnect(state);
        }

        protected override void ClientLogin(TelNetState state)
        {
            EventSink.OnLogin(state);
        }

        protected override void ClientLogout(TelNetState state)
        {
            EventSink.OnLogout(state);
        }
    }
}
