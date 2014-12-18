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
#if NET45
        System.Threading.Tasks.Task WriteAsync(PacketBuffer buffer);
#endif
    }

    public class TCPPacketWriter : BaseStreamWriter, IPacketWriter
    {
#if NET45
        public async System.Threading.Tasks.Task WriteAsync(PacketBuffer buffer)
        {
            await WriteAsync(buffer.GetData());
        }
#endif
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
#if NET45
        public async System.Threading.Tasks.Task WriteAsync(PacketBuffer buffer)
        {
            await WriteAsync(buffer.GetData());
        }
#endif
    }
}
