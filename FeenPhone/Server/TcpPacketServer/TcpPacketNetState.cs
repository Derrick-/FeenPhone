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
        public TcpPacketNetState(Stream stream, IPEndPoint ep)
            : base(stream, ep)
        {
            Handler = new ServerPacketHandler(this);
            Reader.OnReadData += OnRead;
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
            // Send Login Packet
        }

        public void OnUserLogout(Alienseed.BaseNetworkServer.Accounting.IUserClient client)
        {
            // Send Logout Packet
        }

        public void OnChat(Alienseed.BaseNetworkServer.INetState user, string text)
        {
            Packet.WriteChat(Writer, text);
        }
    }
}
