using Alienseed.BaseNetworkServer.Accounting;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Alienseed.BaseNetworkServer.Network.Telnet
{
    public abstract class BaseTelnetServer<Tnetstate> : BaseTCPServer<NetworkTextReader, NetworkTextWriter> where Tnetstate : BaseTelNetState
    {

        public new static IEnumerable<BaseTelNetState> Clients
        {
            get { return NetworkServer.Clients.Where(m => m is BaseTelNetState).Cast<BaseTelNetState>(); }
        }

        protected override void PurgeAllClients()
        {
            List<BaseTelNetState> clients = new List<BaseTelNetState>(Clients);
            foreach (var client in clients)
                client.Dispose();
        }

        public BaseTelnetServer(int port=23, IPAddress address = null) : base(port, address)
        {
            OnClientConnected += svr_OnClientConnected;
        }

        public new static IEnumerable<IUser> AllUsers { get { return BaseTelnetServer<Tnetstate>.Clients.Select(m => m.User); } }

        protected override TCPNetState<NetworkTextReader, NetworkTextWriter> CreateNetstate(System.Net.Sockets.NetworkStream stream, EndPoint ep)
        {
            return NetstateFactory(stream, ep);
            //return new Tnetstate(stream, ep as IPEndPoint);
        }
        protected abstract Tnetstate NetstateFactory(System.Net.Sockets.NetworkStream stream, EndPoint ep);

        void svr_OnClientConnected(TCPNetState<NetworkTextReader, NetworkTextWriter> client)
        {
            if (client is BaseTelNetState)
            {
                BaseTelNetState telclient = (BaseTelNetState)client;
            }
            else
                client.Dispose();
        }
    }
}
