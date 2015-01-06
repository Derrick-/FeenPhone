using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone.Client
{
    abstract class BaseClient : IDisposable
    {
        protected readonly IUserClient _LocalUser;
        public IUser LocalUser { get { return _LocalUser; } }

        internal string Password { get; set; }

        public BaseClient(IUserClient localUser)
        {
            this._LocalUser = localUser;
        }

        public abstract bool IsConnected { get; }

        public abstract void Dispose();

        internal abstract void SendChat(string text);
        internal abstract void SendAudio(Audio.Codecs.CodecID codec, byte[] data, int dataLen);
        internal abstract void SendLoginInfo();
        internal abstract void SendPingReq();
        internal abstract void SendPingResp(ushort timestamp);
    }
}
