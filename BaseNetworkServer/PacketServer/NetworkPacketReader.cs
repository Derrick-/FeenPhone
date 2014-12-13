using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alienseed.BaseNetworkServer.PacketServer
{
    public class NetworkPacketReader : BaseStreamReader
    {

        public class DataReadEventArgs : EventArgs
        {
            public Queue<byte> data { get; set; }
            public DataReadEventArgs(Queue<byte> data)
            {
                this.data = data;
            }
        }

        public event EventHandler<DataReadEventArgs> OnReadData;

        protected override void OnRead()
        {
            if (OnReadData != null)
            {
                OnReadData(this, new DataReadEventArgs(InStream));
            }
        }
    }
}
