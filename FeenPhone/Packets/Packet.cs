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
        Audio = 16,
        PingReq = 22,
        PingResp = 23,
    }

    static class Packet
    {
        internal static void WriteLoginStatus(IPacketWriter Writer, bool isLoggedIn)
        {
            using (var buffer = new FeenPacketBuffer(1))
            {
                buffer.Write(PacketID.LoginStatus);
                buffer.WriteLength(1);
                buffer.Write(isLoggedIn);

                if (Writer != null)
                    Writer.Write(buffer);
            }
        }

        internal static void WriteLoginRequest(IPacketWriter Writer, string username, string password)
        {
            if (username.Contains('\t') || password.Contains('\t'))
                throw new ArgumentException("Username or password contains invalid character");

            string textdata = string.Format("{0}\t{1}", username, password);
            byte[] data = Encoding.ASCII.GetBytes(textdata);

            using (var buffer = new FeenPacketBuffer(PacketID.LoginRequest, data))
            {
                if (Writer != null)
                    Writer.Write(buffer);
            }
        }

        internal static void WriteUserLogin(IPacketWriter Writer, IUser user)
        {
            using (var buffer = new FeenPacketBuffer(PacketID.UserLogin, UserData(user)))
            {
                if (Writer != null)
                    Writer.Write(buffer);
            }
        }

        internal static void WriteUserLogout(IPacketWriter Writer, IUser user)
        {
            using (var buffer = new FeenPacketBuffer(PacketID.UserLogout, UserData(user)))
            {
                if (Writer != null)
                    Writer.Write(buffer);
            }
        }

        internal static void WriteChat(IPacketWriter Writer, IUser user, string text)
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

                if (Writer != null)
                    Writer.Write(buffer);
            }
        }

        internal static void WriteUserList(IPacketWriter Writer, IEnumerable<IUser> users)
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

                if (Writer != null)
                    Writer.Write(buffer);
            }
        }

        internal static void WriteAudioData(IPacketWriter Writer, Guid userID, Audio.Codecs.CodecID Codec, byte[] AudioData, int AudioDataLen)
        {
            using (var buffer = new FeenPacketBuffer())
            {
                buffer.Write(PacketID.Audio);
                buffer.WriteLength(AudioDataLen + (userID == Guid.Empty ? 1 : 17) + 1);
                if (userID == Guid.Empty)
                    buffer.Write(true);
                else
                {
                    buffer.Write(false);
                    buffer.Write(userID.ToByteArray());
                }
                buffer.WriteLength(Codec);
                buffer.Write(AudioData, AudioDataLen);

                if (Writer != null)
                    Writer.Write(buffer);
            }
        }

        internal static void WritePingReq(IPacketWriter Writer)
        {
            var timestamp = (int)Timekeeper.Elapsed.TotalMilliseconds;
            using (var buffer = new FeenPacketBuffer(PacketID.PingReq, new byte[] { (byte)(timestamp >> 8), (byte)(timestamp) }))
            {
                if (Writer != null)
                    Writer.Write(buffer);
            }
        }

        internal static void WritePingResp(IPacketWriter Writer, ushort timestamp)
        {
            using (var buffer = new FeenPacketBuffer(PacketID.PingResp, new byte[] { (byte)(timestamp >> 8), (byte)(timestamp) }))
            {
                if (Writer != null)
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
