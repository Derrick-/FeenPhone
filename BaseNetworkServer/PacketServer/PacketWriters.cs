using Alienseed.BaseNetworkServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alienseed.BaseNetworkServer.PacketServer
{
    public interface IPacketWriter : INetworkWriter
    {
        void Write(PacketBuffer buffer);
    }

    public class TCPPacketWriter : BaseStreamWriter, IPacketWriter
    {
        public void Write(PacketBuffer buffer)
        {
            Write(buffer.GetData());
        }
    }

    public class UDPPacketWriter : BaseUDPWriter, IPacketWriter
    {
        public void Write(PacketBuffer buffer)
        {
            Write(buffer.GetData());
        }
    }
}
