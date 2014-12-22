using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone.Client
{
    class LocalClient : BaseClient, FeenPhone.Server.IFeenPhoneNetstate
    {
        public Server.IFeenPhoneClientNotifier Notifier { get; private set; }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public ushort LastPing { get { return 0; } set { throw new NotImplementedException(); } }

        public LocalClient(IUserClient localUser) : base(localUser)
        {
            Notifier = new LocalNotifier();
        }

        public override bool IsConnected
        {
            get { return true; }
        }

        public class LocalNotifier : Server.IFeenPhoneClientNotifier
        {
            public void OnChat(Alienseed.BaseNetworkServer.INetState client, string text)
            {
                EventSource.InvokeOnChat(this, client.User, text);
            }

            public void OnAudio(Guid userID, Audio.Codecs.CodecID Codec, byte[] data, int dataLen)
            {
                EventSource.InvokeOnAudio(this, userID, Codec, data, dataLen);
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

            bool Server.IFeenPhoneClientNotifier.Login(string username, string password)
            {
                throw new NotImplementedException();
            }

            void Server.IFeenPhoneClientNotifier.LoginSuccess()
            {
                throw new NotImplementedException();
            }

            void Server.IFeenPhoneClientNotifier.LoginFailed()
            {
                throw new NotImplementedException();
            }
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
            Server.EventSink.OnAudio(this, Guid.Empty, codec, data, dataLen);
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

        public bool Login(string username, string password)
        {
            return true;
        }


        public bool LoginSetUser(IUserClient user, bool dcIfLoggedIn)
        {
            throw new NotImplementedException();
        }

        public void LogLine(string format, object arg0)
        {
            LogLine(string.Format(format, arg0));
        }

        public void LogLine(string format, params object[] args)
        {
            LogLine(string.Format(format, args));
        }

        public void LogLine(string text)
        {
            Console.Write("{Local}: [{LocalClient}] {0}", text);
        }
    }
}
