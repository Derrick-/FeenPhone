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
            Reader.OnDisconnect += Reader_OnDisconnect;
            Reader.OnBufferOverflow += Reader_OnBufferOverflow;
        }

        void Reader_OnBufferOverflow(object sender, Alienseed.BaseNetworkServer.BufferOverflowArgs e)
        {
            Console.WriteLine("Client Buffer Overflow: Truncating Buffer");
            e.handled = true;
        }

        void Reader_OnDisconnect()
        {
            Disconnect();
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
            EventSource.InvokeOnUserList(null, null);
            Writer.SetStream(null);
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

        internal override void SendChat(string text)
        {
            Packet.WriteChat(Writer, LocalUser, text);
            EventSource.InvokeOnChat(this, LocalUser, text);
        }

        internal override void SendAudio(Audio.Codecs.CodecID codec, byte[] data, int dataLen)
        {
            Packet.WriteAudioData(Writer, codec, data, dataLen);
        }

        internal override void SendLoginInfo()
        {
            Console.WriteLine("Logging in as {0}", LocalUser.Nickname);
            Packet.WriteLoginRequest(Writer, LocalUser.Nickname, LocalUser.Nickname);
        }
    }
}
