using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone.Client
{
    class LocalClient : BaseClient, FeenPhone.Server.IFeenPhoneEvents, Alienseed.BaseNetworkServer.INetState
    {
        public LocalClient(IUserClient localUser) : base(localUser) { }

        public override bool IsConnected
        {
            get { return true; }
        }

        public void OnChat(Alienseed.BaseNetworkServer.INetState client, string text)
        {
            EventSource.InvokeOnChat(this, client.User, text);
        }

        public void OnAudio(Audio.Codecs.CodecID Codec, byte[] data, int dataLen)
        {
            EventSource.InvokeOnAudio(this, Codec, data, dataLen);
        }

        public void OnUserLogin(Alienseed.BaseNetworkServer.Accounting.IUserClient client)
        {
            EventSource.InvokeOnUserConnected(this, client);
        }

        public void OnUserLogout(Alienseed.BaseNetworkServer.Accounting.IUserClient client)
        {
            EventSource.InvokeOnUserDisconnected(this, client);
        }

        public void OnUserConnected(Alienseed.BaseNetworkServer.INetState client)
        {
            //throw new NotImplementedException();
        }

        public void OnUserDisconnected(Alienseed.BaseNetworkServer.INetState client)
        {
            if (client.User != null)
                EventSource.InvokeOnUserDisconnected(this, client.User);
        }

        public override void Dispose()
        {

        }

        internal override void SendChat(string text)
        {
            Server.EventSink.OnChat(this, text);
        }

        internal override void SendAudio(Audio.Codecs.CodecID codec, byte[] data, int dataLen)
        {
            Server.EventSink.OnAudio(this, codec, data, dataLen);
        }

        internal override void SendLoginInfo()
        {
            Server.EventSink.OnLogin(this);
        }


        internal override void SendPingReq()
        {
            throw new NotImplementedException();
        }
        
        internal override void SendPingResp(ushort timestamp)
        {
            // ping to self is zero
        }

        IUserClient IClient.User
        {
            get { return _LocalUser; }
        }

        bool IClient.Connected
        {
            get { return this.IsConnected; }
        }
    }
}
