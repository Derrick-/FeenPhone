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
        Chat = 2,
        LoginStatus = 5,
        LoginRequest = 6,
        UserList = 12,
        UserLogout = 14,
        UserLogin = 15,
        Audio = 16
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

        internal static void WriteUserLogin(NetworkPacketWriter Writer, IUser user)
        {
            using (var buffer = new FeenPacketBuffer(PacketID.UserLogin, UserData(user)))
            {
                Writer.Write(buffer);
            }
        }

        internal static void WriteUserLogout(NetworkPacketWriter Writer, IUser user)
        {
            using (var buffer = new FeenPacketBuffer(PacketID.UserLogout, UserData(user)))
            {
                Writer.Write(buffer);
            }
        }

        internal static void WriteChat(NetworkPacketWriter Writer, IUser user, string text)
        {
            byte[] dataText = Encoding.ASCII.GetBytes(text);

            using (var buffer = new FeenPacketBuffer())
            {
                byte[] dataUser = UserData(user);
                var len = dataUser.Length + dataText.Length;

                buffer.Write(PacketID.Chat);

                buffer.WriteLength(len);

                buffer.Write(dataUser);

                buffer.Write(dataText);

                Writer.Write(buffer);
            }
        }

        internal static void WriteUserList(NetworkPacketWriter Writer, IEnumerable<IUser> users)
        {
            if (users.Count() > byte.MaxValue)
                throw new ArgumentException("Too many users, max is " + byte.MaxValue);

            List<byte[]> usersDatas = new List<byte[]>();

            foreach (var user in users)
                usersDatas.Add(UserData(user));

            using (var buffer = new FeenPacketBuffer())
            {
                buffer.Write(PacketID.UserList);
                buffer.WriteLength(usersDatas.Sum(m => m.Length) + 1);
                buffer.Write((byte)usersDatas.Count());
                buffer.Write(usersDatas.SelectMany(m => m).ToArray());

                Writer.Write(buffer);
            }
        }

        internal static void WriteAudioData(NetworkPacketWriter Writer, Audio.Codecs.CodecID Codec, byte[] AudioData, int AudioDataLen)
        {
            using (var buffer = new FeenPacketBuffer())
            {
                buffer.Write(PacketID.Audio);
                buffer.WriteLength(AudioDataLen + 1);
                buffer.WriteLength(Codec);
                buffer.Write(AudioData, AudioDataLen);

                Writer.Write(buffer);
            }
        }

        private static byte[] UserData(IUser user)
        {
            if (user.Username.Contains('\t') || user.Nickname.Contains('\t'))
                throw new ArgumentException("Username or Nickname contains invalid character");

            string textdata = string.Format("{0}\t{1}\t{2}\t{3}", user.ID, user.IsAdmin ? "1" : "0", user.Username, user.Nickname);
            return (new byte[] { (byte)(textdata.Length >> 8), (byte)textdata.Length }).Concat(Encoding.ASCII.GetBytes(textdata)).ToArray();
        }
    }
}
