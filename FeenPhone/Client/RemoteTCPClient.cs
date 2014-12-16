using Alienseed.BaseNetworkServer.Accounting;
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

        public RemoteTCPClient(IUserClient localUser, System.Net.IPAddress IP, int port) : base(localUser, IP, port) { }

        volatile bool connecting = false;
        public override void Connect()
        {
            if (!connecting)
            {
                TcpClient client = new TcpClient();
                connecting = true;
                Console.WriteLine("Connecting to {0}...", IP);
                client.BeginConnect(IP.ToString(), Port, new AsyncCallback(ConnectCallback), client);
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
                Reader.SetStream(Stream, readerBufferSize);

                Writer.SetStream(Stream);
                _IsConnected = true;
            }
            connecting = false;
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
