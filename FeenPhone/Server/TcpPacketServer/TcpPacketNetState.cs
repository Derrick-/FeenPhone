using Alienseed.BaseNetworkServer;
using Alienseed.BaseNetworkServer.PacketServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace FeenPhone.Server.TcpPacketServer
{
    class TcpPacketNetState : BasePacketNetState, IFeenPhoneNetstate
    {

        private readonly ServerPacketHandler Handler;
        public TcpPacketNetState(Stream stream, IPEndPoint ep, int readBufferSize)
            : base(stream, ep, readBufferSize)
        {
            Handler = new ServerPacketHandler(this);
            Reader.PreviewIncoming += Reader_PreviewIncoming;
            Reader.OnReadData += OnRead;
        }

        protected override void Reader_OnBufferOverflow(object sender, BufferOverflowArgs e)
        {
            Console.WriteLine("Buffer overflow from {0}", this.ClientIdentifier);
            e.handled = true;
        }

        protected void OnRead(object sender, Alienseed.BaseNetworkServer.PacketServer.NetworkPacketReader.DataReadEventArgs args)
        {
            Queue<byte> InStream = args.data;

            if (InStream.Count > 0)
            {
                byte[] bytes = new byte[InStream.Count];

                InStream.CopyTo(bytes, 0);
                string read = Encoding.ASCII.GetString(bytes);

                Handler.Handle(InStream);
            }

        }

        public void OnUserConnected(Alienseed.BaseNetworkServer.INetState user)
        {
            // Send Connected Packet
        }

        public void OnUserDisconnected(Alienseed.BaseNetworkServer.INetState user)
        {
            // Send Disconnected Packet
        }

        public void OnUserLogin(Alienseed.BaseNetworkServer.Accounting.IUserClient client)
        {
            Packet.WriteUserLogin(Writer, client);
        }

        public void OnUserLogout(Alienseed.BaseNetworkServer.Accounting.IUserClient client)
        {
            Packet.WriteUserLogout(Writer, client);
        }

        public void OnChat(Alienseed.BaseNetworkServer.INetState user, string text)
        {
            Packet.WriteChat(Writer, user.User, text);
        }

        public void OnAudio(Audio.Codecs.CodecID Codec, byte[] data, int dataLen)
        {
            Packet.WriteAudioData(Writer, Codec, data, dataLen);
        }

        public void OnLoginFailed()
        {
            Packet.WriteLoginStatus(Writer, false);
        }

        public void OnLoginSuccess()
        {
            Packet.WriteLoginStatus(Writer, true);

            var users = NetworkServer.AllUsers.Where(m => m != null);
            if (ServerHost.LocalClient != null)
            {
                users = users.Concat(new Alienseed.BaseNetworkServer.Accounting.IUser[] { ServerHost.LocalClient.LocalUser });
            }
            Packet.WriteUserList(Writer, users.Where(m => m.ID != this.User.ID));
        }

        internal bool Login(string Username, string password)
        {
            var user = FeenPhone.Accounting.AccountHandler.Login(Username, password);

            LogLine("Login {0}: {1}", user != null ? "SUCCESS" : "FAILED", user != null ? user.Username : Username);

            if (user == null) return false;

            return LoginSetUser(user);
        }
    }
}
