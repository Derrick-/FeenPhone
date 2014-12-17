using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Alienseed.BaseNetworkServer.PacketServer
{
    public abstract class BaseUDPPacketServer<Tnetstate> : BaseUDPServer<UDPPacketReader, UDPPacketWriter, Tnetstate> where Tnetstate : BaseUdpPacketNetState
    {
        public new static IEnumerable<Tnetstate> Clients { get { return BaseServer.Clients.Where(m => m is Tnetstate).Cast<Tnetstate>(); } }
        public new static IEnumerable<IUser> Users { get { return BaseUDPPacketServer<Tnetstate>.Clients.Select(m => m.User); } }

        public BaseUDPPacketServer(int port)
            : base(port)
        {
        }

        protected override void PurgeAllClients()
        {
            foreach (var client in Clients.ToList())
                client.Dispose();
        }

    }
}
