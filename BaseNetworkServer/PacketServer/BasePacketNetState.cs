using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Alienseed.BaseNetworkServer.PacketServer
{
    public abstract class BaseUdpPacketNetState : UDPNetState<UDPPacketReader, UDPPacketWriter>
    {
        protected override string LogTitle { get { return "UDP PacketServer"; } }

        public BaseUdpPacketNetState(IPEndPoint ep, int readBufferSize) : base(ep, readBufferSize)
        {
        }

   }
  
    public abstract class BaseTcpPacketNetState : TCPNetState<TCPPacketReader, TCPPacketWriter>
    {
        protected override string LogTitle { get { return "TCP PacketServer"; } }

        public BaseTcpPacketNetState(System.Net.Sockets.NetworkStream stream, IPEndPoint ep, int readBufferSize) : base(stream, ep, readBufferSize)
        {

        }
    }
}
