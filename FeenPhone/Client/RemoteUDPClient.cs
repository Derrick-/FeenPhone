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

        volatile bool _isconnecting = false;
        public override void Connect()
        {
            _isconnecting = true;
            var client = new UdpClient();
            Console.WriteLine("Connecting UDP to {0}...", HostIP);

            var ep = new IPEndPoint(HostIP, Port);
            client.Connect(ep);
            client.Send(new byte[] { 1 }, 1);
            client.BeginReceive(new AsyncCallback(ConnectCallback), client);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                var client = ar.AsyncState as UdpClient;
                IPEndPoint endpoint = new IPEndPoint(HostIP, 0);
                byte[] data = client.EndReceive(ar, ref endpoint);

                if (_isconnecting && data.Length == 2)
                {
                    int port = data[0] << 8 | data[1];
                    IPEndPoint localep = client.Client.LocalEndPoint as IPEndPoint;
                    client.Close();
                    if (Client != null)
                        Client.Close();
                    Client = new UdpClient(localep);
                    Client.Connect(endpoint.Address, port);
                    _writer.SetClient(Client);
                    Client.BeginReceive(new AsyncCallback(RecieveCallback), null);
                    Console.WriteLine("Connected UDP.");
                    SendLoginInfo();
                    return;
                }
            }
            catch (SocketException)
            {
                Disconnect();
                return;
            }
            catch (ObjectDisposedException)
            {
                Disconnect();
                return;
            }
            _isconnecting = false;
        }

        private void RecieveCallback(IAsyncResult ar)
        {
            IPEndPoint endpoint = new IPEndPoint(HostIP, 0);
            if (Client != null)
            {
                byte[] data;
                try
                {
                    data = Client.EndReceive(ar, ref endpoint);
                }
                catch (SocketException)
                {
                    Disconnect();
                    return;
                }
                catch (ObjectDisposedException)
                {
                    Disconnect();
                    return;
                }
                Client.BeginReceive(new AsyncCallback(RecieveCallback), null);
                if (HostIP.Equals(endpoint.Address))
                {
                    _reader.ReceivedData(data);
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
            _isconnecting = false;
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
