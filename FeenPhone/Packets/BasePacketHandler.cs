using Alienseed.BaseNetworkServer.Accounting;
using FeenPhone.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace FeenPhone
{

    abstract class BasePacketHandler
    {
        protected delegate void Handler(IEnumerable<byte> payload);
        Dictionary<PacketID, Handler> Handlers;

        public BasePacketHandler()
        {
            Handlers = new Dictionary<PacketID, Handler>()
                {
                    {PacketID.LoginStatus, Handle_LoginStatus},
                    {PacketID.LoginRequest, Handle_LoginInfo},
                    {PacketID.UserLogin, Handle_UserLogin},
                    {PacketID.UserLogout, Handle_UserLogout},
                    {PacketID.UserList, Handle_UserList},
                    {PacketID.Chat, Handle_OnChat},
                    {PacketID.Audio, Handle_OnAudio},
                    {PacketID.PingReq, Handle_PingReq},
                    {PacketID.PingResp, Handle_PingResp},
                };
        }

        public bool ValidPacketID(byte id) { return ValidPacketID((PacketID)id); }
        public bool ValidPacketID(PacketID id) { return Handlers.ContainsKey(id); }

        public void Handle(Queue<byte> data)
        {
            Handler handler;
            ushort len;
            int consumed = Parse(data.ToArray(), out handler, out len);
            while (handler != null)
            {
                if (handler != null)
                {
                    //Trace.WriteLine(string.Format("BasePacketHandler: Handler:{0} Bytes:{1}, Queue.Count:{2}", handler.Method.Name, len, data.Count()), "Network");
                    handler(data.Skip(3).Take(len));
                }

                for (int i = 0; i < consumed && data.Any(); i++)
                {
                    data.Dequeue();
                }

                consumed = Parse(data.ToArray(), out handler, out len);
            }
        }

        private int Parse(byte[] data, out Handler handler, out ushort len)
        {
            handler = null;
            len = 0;
            int i = 0;
            while (i < data.Length - 2 && !ValidPacketID(data[i]))
            {
                i++;
            }

            if (data.Length - i < 3 || !ValidPacketID(data[i]))
            {
                return i;
            }

            PacketID packetid = (PacketID)data[i];
            len = (ushort)((data[i + 1] << 8) | data[i + 2]);
            if (data.Length < len + 3)
                return i;
            IEnumerable<byte> payload = data.Skip(3).Take(len);

            handler = Handlers[packetid];
            return len + 3;
        }

        protected abstract void OnLoginStatus(bool isLoggedIn);
        protected void Handle_LoginStatus(IEnumerable<byte> payload)
        {
            if (payload.Count() != 1)
                throw new ArgumentException("Invalid LoginStatus packet length");

            OnLoginStatus(payload.Single() == 0 ? false : true);
        }

        protected abstract void LoginInfo(string username, string password);
        protected void Handle_LoginInfo(IEnumerable<byte> payload)
        {
            string[] values = Encoding.ASCII.GetString(payload.ToArray()).Split('\t');
            if (values.Length == 2)
            {
                var username = values[0];
                var password = values[1];
                LoginInfo(username, password);
            }
        }

        protected abstract void UserLogin(IUser user);
        protected void Handle_UserLogin(IEnumerable<byte> payload)
        {
            IUser user;
            try
            {
                ReadUser(payload, out user);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("Handle_UserLogin: {0}", ex.Message);
                return;
            }
            UserLogin(user);
        }

        protected abstract void UserLogout(IUser user);
        protected void Handle_UserLogout(IEnumerable<byte> payload)
        {
            IUser user;
            try
            {
                ReadUser(payload, out user);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("Handle_UserLogout: {0}", ex.Message);
                return;
            }
            UserLogout(user);
        }

        protected abstract void OnChat(IUser user, string text);
        protected void Handle_OnChat(IEnumerable<byte> payload)
        {
            IUser user;
            int consumed = ReadUser(payload, out user);
            string text = Encoding.ASCII.GetString(payload.Skip(consumed).ToArray());
            OnChat(user, text);
        }

        protected abstract void OnUserList(IEnumerable<IUser> users);
        protected void Handle_UserList(IEnumerable<byte> payload)
        {
            int count = payload.First();
            List<IUser> users = new List<IUser>(count);
            int consumed = 1;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    IUser user;
                    try
                    {
                        consumed += ReadUser(payload.Skip(consumed), out user);
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine("Handle_UserList: {0}", ex.Message);
                        return;
                    }
                    users.Add(user);
                }
            }
            OnUserList(users);
        }

        protected abstract void OnAudio(Audio.Codecs.CodecID Codec, byte[] data, int dataLen);
        protected void Handle_OnAudio(IEnumerable<byte> payload)
        {
            if (!payload.Any())
                throw new ArgumentException("No audio payload");

            Audio.Codecs.CodecID Codec = (Audio.Codecs.CodecID)payload.First();
            OnAudio(Codec, payload.Skip(1).ToArray(), payload.Count() - 1);
        }

        protected abstract void OnPingReq(ushort timestamp);
        protected void Handle_PingReq(IEnumerable<byte> payload)
        {
            ushort timestamp = ReadPingTimestamp(payload);
            OnPingReq(timestamp);
        }

        protected abstract void OnPingResp(ushort elapsed);
        protected void Handle_PingResp(IEnumerable<byte> payload)
        {
            ushort timestamp = ReadPingTimestamp(payload);
            int now = (int)Timekeeper.Elapsed.TotalMilliseconds;
            ushort elapsed;
            unchecked { elapsed = (ushort)(now - timestamp); }
            OnPingResp(elapsed);
        }

        private static ushort ReadPingTimestamp(IEnumerable<byte> payload)
        {
            var count = payload.Count();

            if (count < 2)
                throw new ArgumentException("Out of data in ReadPingTimestamp");

            byte[] tsBytes = payload.Take(2).ToArray();
            ushort timestamp = (ushort)(tsBytes[0] << 8 | tsBytes[1]);
            return timestamp;
        }

        protected abstract IUser GetUserObject(Guid id, bool isadmin, string username, string nickname);
        private int ReadUser(IEnumerable<byte> payload, out IUser user)
        {
            var count = payload.Count();

            if (count < 2)
                throw new ArgumentException("Out of data in ReadUser");

            byte[] lenBytes = payload.Take(2).ToArray();
            ushort len = (ushort)(lenBytes[0] << 8 | lenBytes[1]);

            if (len <= 0 || count < 2 + len)
                throw new ArgumentException("Out of data in ReadUser");

            byte[] userData = payload.Skip(2).Take(len).ToArray();

            string[] uservalues = Encoding.ASCII.GetString(userData).Split('\t');

            if (uservalues.Length != 4)
                throw new ArgumentException("Invalid user data item count in ReadUser");

            Guid id;
            if (!Guid.TryParse(uservalues[0], out id))
                throw new ArgumentException("Invalid user id item count in ReadUser");

            bool isadmin = uservalues[1] == "1";
            string username = uservalues[2];
            string nickname = uservalues[3];

            user = GetUserObject(id, isadmin, username, nickname);

            return len + 2;
        }


    }
}
