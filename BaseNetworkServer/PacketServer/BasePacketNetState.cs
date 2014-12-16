using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Alienseed.BaseNetworkServer.PacketServer
{
    public abstract class BasePacketNetState : TCPNetState<NetworkPacketReader, NetworkPacketWriter>
    {
        protected override string LogTitle { get { return "PacketServer"; } }

        public BasePacketNetState(Stream stream, IPEndPoint ep, int readBufferSize) : base(stream, ep, readBufferSize)
        {

        }
    }
}
