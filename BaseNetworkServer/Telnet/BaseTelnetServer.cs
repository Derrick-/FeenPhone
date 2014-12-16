using Alienseed.BaseNetworkServer.Accounting;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace  Alienseed.BaseNetworkServer.Telnet
{
    public abstract class BaseTelnetServer<Tnetstate> : BaseTCPServer<NetworkTextReader, NetworkTextWriter, Tnetstate> where Tnetstate : BaseTelNetState
    {

        public new static IEnumerable<Tnetstate> Clients { get { return BaseServer.Clients.Where(m => m is Tnetstate).Cast<Tnetstate>(); } }
        public new static IEnumerable<IUser> Users { get { return BaseTelnetServer<Tnetstate>.Clients.Select(m => m.User); } }

        protected override void PurgeAllClients()
        {
            foreach (var client in Clients.ToList())
                client.Dispose();
        }

        public BaseTelnetServer(int port=23, IPAddress address = null) : base(port, address)
        {
            OnClientConnected += ValidateNetstate;
        }

        protected sealed override Tnetstate CreateNetstate(System.Net.Sockets.NetworkStream stream, EndPoint ep)
        {
            return NetstateFactory(stream, ep);
        }
        protected abstract Tnetstate NetstateFactory(System.Net.Sockets.NetworkStream stream, EndPoint ep);

        private void ValidateNetstate(NetState client)
        {
            if (client is Tnetstate)
            {
                Tnetstate telclient = (Tnetstate)client;
            }
            else
                client.Dispose();
        }
    }
}
