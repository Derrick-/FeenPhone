using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Alienseed.BaseNetworkServer.PacketServer
{
    public abstract class BaseTCPPacketServer<Tnetstate> : BaseTCPServer<TCPPacketReader, TCPPacketWriter, Tnetstate> where Tnetstate : BaseTcpPacketNetState
    {
        public new static IEnumerable<Tnetstate> Clients { get { return BaseServer.Clients.Where(m => m is Tnetstate).Cast<Tnetstate>(); } }
        public new static IEnumerable<IUser> Users { get { return BaseTCPPacketServer<Tnetstate>.Clients.Select(m => m.User); } }

        public BaseTCPPacketServer(int port, IPAddress address, bool noDelay = false, bool useDirectSocketWrite = false) : base(port, address, noDelay, useDirectSocketWrite)
        {
        }
        
        protected override void PurgeAllClients()
        {
            foreach (var client in Clients.ToList())
                client.Dispose();
        }

        protected sealed override Tnetstate CreateNetstate(System.Net.Sockets.NetworkStream stream, System.Net.EndPoint ep)
        {
            return NetstateFactory(stream, ep);
        }
        protected abstract Tnetstate NetstateFactory(System.Net.Sockets.NetworkStream stream, EndPoint ep);

    }
}
