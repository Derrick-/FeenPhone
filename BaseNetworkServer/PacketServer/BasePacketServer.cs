using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Alienseed.BaseNetworkServer.PacketServer
{
    abstract class BasePacketServer<Tnetstate> : BaseTCPServer<NetworkPacketReader, NetworkPacketWriter> where Tnetstate : BasePacketNetState
    {
        public new static IEnumerable<Tnetstate> Clients { get { return NetworkServer.Clients.Where(m => m is Tnetstate).Cast<Tnetstate>(); } }
        public new static IEnumerable<IUser> AllUsers { get { return BasePacketServer<Tnetstate>.Clients.Select(m => m.User); } }

        public BasePacketServer(int port, IPAddress address) : base(port,address)
        {
            OnClientConnected += ValidateNetstate;
        }
        
        protected override void PurgeAllClients()
        {
            foreach (var client in Clients.ToList())
                client.Dispose();
        }

        protected sealed override TCPNetState<NetworkPacketReader, NetworkPacketWriter> CreateNetstate(System.Net.Sockets.NetworkStream stream, System.Net.EndPoint ep)
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
