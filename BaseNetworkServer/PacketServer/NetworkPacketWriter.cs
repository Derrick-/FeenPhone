using Alienseed.BaseNetworkServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alienseed.BaseNetworkServer.PacketServer
{
    public class NetworkPacketWriter : BaseStreamWriter
    {
        public void Write(PacketBuffer buffer)
        {
            Write(buffer.GetData());
        }
    }
}
