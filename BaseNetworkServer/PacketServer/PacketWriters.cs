using Alienseed.BaseNetworkServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alienseed.BaseNetworkServer.PacketServer
{
    public interface IPacketWriter : INetworkWriter
    {
        void Write(IPacketBuffer buffer);
    }

    public class TCPPacketWriter : BaseStreamWriter, IPacketWriter
    {
        public void Write(IPacketBuffer buffer)
        {
            Write(buffer.GetData(), buffer.BytesLength);
        }
    }

    public class UDPPacketWriter : BaseUDPWriter, IPacketWriter
    {
        public void Write(IPacketBuffer buffer)
        {
            Write(buffer.GetData(), buffer.BytesLength);
        }
    }
}
