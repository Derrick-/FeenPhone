using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace FeenPhone.Client
{
    class RemoteClient : BaseClient
    {
        const int bufferSize = 1024;

        private readonly System.Net.IPAddress IP;
        private readonly int Port;

        private readonly ClientPacketHandler Handler;

        TcpClient Client = null;
        Alienseed.BaseNetworkServer.PacketServer.NetworkPacketReader Reader = new Alienseed.BaseNetworkServer.PacketServer.NetworkPacketReader();
        NetworkStream Stream = null;
        Alienseed.BaseNetworkServer.PacketServer.NetworkPacketWriter Writer = new Alienseed.BaseNetworkServer.PacketServer.NetworkPacketWriter();

        public RemoteClient(IUserClient localUser, System.Net.IPAddress IP, int port)
            : base(localUser)
        {
            this.IP = IP;
            this.Port = port;
            Handler = new ClientPacketHandler();

            Reader.OnReadData += Reader_OnReadData;
        }

        void Reader_OnReadData(object sender, Alienseed.BaseNetworkServer.PacketServer.NetworkPacketReader.DataReadEventArgs e)
        {
            Handler.Handle(e.data);
        }

        volatile bool connecting = false;
        public void Connect()
        {
            if (!connecting)
            {
                TcpClient client = new TcpClient();
                connecting = true;
                client.BeginConnect(IP.ToString(), Port, new AsyncCallback(ConnectCallback), client);
            }
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
                Reader.SetStream(Stream);
                Writer.SetStream(Stream);
                _IsConnected = true;
            }
            connecting = false;
        }

        private void ConnectionFailed(string message)
        {
            Console.WriteLine("Connection failed: {0}", message);
        }

        private void Disconnect()
        {
            Writer.SetStream(null);
            if (Stream != null)
                Stream.Dispose();
            Client.Close();
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

        internal override void SendChat(string text)
        {
            Packet.WriteChat(Writer, text);
            OnChat(LocalUser, text);
        }
    }
}
