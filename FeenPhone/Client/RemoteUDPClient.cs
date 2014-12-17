using Alienseed.BaseNetworkServer.Accounting;
using Alienseed.BaseNetworkServer.PacketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FeenPhone.Client
{
    class RemoteUDPClient : RemoteClient
    {
        UdpClient Client = null;
        protected NetworkStream Stream = null;

        public RemoteUDPClient(IUserClient localUser, IPAddress IP, int port) : base(localUser, IP, port) { }

        UDPPacketReader _reader = new UDPPacketReader();
        protected override IPacketReader Reader { get { return _reader; } }

        UDPPacketWriter _writer = new UDPPacketWriter();
        protected override IPacketWriter Writer { get { return _writer; } }

        public override void Connect()
        {
            Client = new UdpClient();
            Console.WriteLine("Connecting to {0}...", HostIP);

            var ep = new IPEndPoint(HostIP, Port);
            Client.Connect(ep);
            Listen();

            _writer.SetEndpoint(ep);
            Packet.WriteLoginRequest(Writer, LocalUser.Username, LocalUser.Username);

        }

        private void Listen()
        {
            if (Client != null)
            {
                Client.BeginReceive(new AsyncCallback(RecieveCallback), this);
            }
        }

        private void RecieveCallback(IAsyncResult ar)
        {
            IPEndPoint endpoint = new IPEndPoint(HostIP, 0);
            if (Client != null)
            {
                var data = Client.EndReceive(ar, ref endpoint);
                if (endpoint.Address == HostIP)
                {
                    _reader.ReceivedData(data);
                    Listen();
                }
            }
        }

        protected override void Disconnect()
        {
            base.Disconnect();

            if (Client != null)
            {
                Console.WriteLine("Disconnected.");
                Client.Close();
            }
            Client = null;
        }

        public override bool IsConnected
        {
            get { return Client != null; }
        }

        public override void Dispose()
        {
            if (Client != null)
            {
                Disconnect();
            }
            _writer.Dispose();
            _reader.Dispose();
        }
    }
}
