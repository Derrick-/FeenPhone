using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alienseed.BaseNetworkServer.PacketServer
{
    interface IPacketReader
    {
        event EventHandler<DataReadEventArgs> OnReadData;
    }

    public class TCPPacketReader : BaseStreamReader, IPacketReader
    {
        public event EventHandler<DataReadEventArgs> OnReadData;

        protected override void OnRead()
        {
            if (OnReadData != null)
            {
                OnReadData(this, new DataReadEventArgs(InStream));
            }
        }
    }

    public class UDPPacketReader : BaseUDPReader, IPacketReader
    {
        public void ReceivedData(byte[] data)
        {
            foreach (byte b in data)
                InStream.Enqueue(b);
            OnRead();
        }
    }
}
