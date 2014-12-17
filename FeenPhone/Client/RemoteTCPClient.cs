using Alienseed.BaseNetworkServer.Accounting;
using Alienseed.BaseNetworkServer.PacketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace FeenPhone.Client
{
    class RemoteTCPClient : RemoteClient
    {
        TcpClient Client = null;
        protected NetworkStream Stream = null;

        public RemoteTCPClient(IUserClient localUser, System.Net.IPAddress IP, int port) : base(localUser, IP, port) { }

        TCPPacketReader _reader = new TCPPacketReader();
        protected override IPacketReader Reader { get { return _reader; } }

        TCPPacketWriter _writer = new TCPPacketWriter();
        protected override IPacketWriter Writer { get { return _writer; } }

        volatile bool connecting = false;
        public override void Connect()
        {
            if (!connecting)
            {
                TcpClient client = new TcpClient();
                connecting = true;
                Console.WriteLine("Connecting to {0}...", HostIP);
                client.BeginConnect(HostIP.ToString(), Port, new AsyncCallback(ConnectCallback), client);
            }
            else
                Console.WriteLine("Already connecting...");
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            TcpClient client = ar.AsyncState as TcpClient;
            if (client != null)
            {
                try
                {
                    client.EndConnect(ar);
                }
                catch (SocketException ex)
                {
                    connecting = false;
                    ConnectionFailed(ex.Message);
                    return;
                }
                Client = client;
                Stream = Client.GetStream();
                _reader.SetStream(Stream, readerBufferSize);

                _writer.SetStream(Stream);
                _IsConnected = true;
                SendLoginInfo();
            }
            connecting = false;
        }

        protected override void Disconnect()
        {
            base.Disconnect();
            _writer.SetStream(null);
            if (Stream != null)
                Stream.Dispose();

            if (Client != null)
            {
                Console.WriteLine("Disconnected.");
                Client.Close();
            }
            Client = null;
        }

        private bool _IsConnected = false;
        public override bool IsConnected
        {
            get { return Client != null && _IsConnected; }
        }

        public override void Dispose()
        {
            if (Client != null)
            {
                Disconnect();
            }
        }
    }
}
