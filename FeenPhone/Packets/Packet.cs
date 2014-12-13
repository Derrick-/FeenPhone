using Alienseed.BaseNetworkServer.PacketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FeenPhone.Packets;
using Alienseed.BaseNetworkServer.Accounting;

namespace FeenPhone
{
    enum PacketID : byte
    {
        LoginStatus = 1,
        LoginRequest = 2,
        Chat = 11
    }

    static class Packet
    {
        internal static void WriteLoginStatus(NetworkPacketWriter Writer, bool isLoggedIn)
        {
            using (var buffer = new FeenPacketBuffer(1))
            {
                buffer.Write(PacketID.LoginStatus);
                buffer.WriteLength(1);
                buffer.Write(isLoggedIn);

                Writer.Write(buffer);
            }
        }

        internal static void WriteLoginRequest(NetworkPacketWriter Writer, string username, string password)
        {
            if (username.Contains('\t') || password.Contains('\t'))
                throw new ArgumentException("Username or password contains invalid character");

            string textdata = string.Format("{0}\t{1}", username, password);
            byte[] data = Encoding.ASCII.GetBytes(textdata);

            using (var buffer = new FeenPacketBuffer(PacketID.LoginRequest, data))
                Writer.Write(buffer);
        }

        internal static void WriteChat(NetworkPacketWriter Writer, IUser user, string text)
        {
            byte[] dataText = Encoding.ASCII.GetBytes(text);

            using (var buffer = new FeenPacketBuffer())
            {
                byte[] dataUser = UserData(user);
                var len = dataUser.Length + 2 + dataText.Length;

                buffer.Write(PacketID.Chat);

                buffer.WriteLength(len);
                
                buffer.WriteLength(dataUser.Length);
                buffer.Write(dataUser);
              
                buffer.Write(dataText);

                Writer.Write(buffer);
            }
        }

        private static byte[] UserData(IUser user)
        {
            if (user.Username.Contains('\t') || user.Nickname.Contains('\t'))
                throw new ArgumentException("Username or Nickname contains invalid character");

            string textdata = string.Format("{0}\t{1}\t{2}\t{3}", user.ID, user.IsAdmin ? "1" : "0", user.Username, user.Nickname);
            return Encoding.ASCII.GetBytes(textdata); 
        }
    }
}
