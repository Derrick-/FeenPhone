using Alienseed.BaseNetworkServer;
using Alienseed.BaseNetworkServer.PacketServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace FeenPhone.Server.PacketServer
{
    class UdpPacketNetState : BaseUdpPacketNetState, IFeenPhonePacketNetState
    {
        public IFeenPhoneClientNotifier Notifier { get; private set; }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        private ushort _LastPing;
        public ushort LastPing
        {
            get { return _LastPing; }
            set
            {
                _LastPing = value;
                InvokePropertyChanged("LastPing");
            }
        }

        private readonly ServerPacketHandler Handler;
        public UdpPacketNetState(IPEndPoint ep, int readBufferSize)
            : base(ep, readBufferSize)
        {
            Handler = new ServerPacketHandler(this);
            Notifier = new PacketClientNotificationHandler(this);
            Reader.OnReadData += Handler.OnRead;
        }

        protected override void Reader_OnBufferOverflow(object sender, BufferOverflowArgs e)
        {
            Console.WriteLine("Buffer overflow from {0}", this.ClientIdentifier);
            e.handled = true;
        }

        private void InvokePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propName));
        }

        IPacketWriter IFeenPhonePacketNetState.Writer
        {
            get { return (IPacketWriter)Writer; }
        }
    }
}
