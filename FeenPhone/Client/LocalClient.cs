using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone.Client
{
    class LocalClient : BaseClient, FeenPhone.Server.IFeenPhoneNetstate, Alienseed.BaseNetworkServer.INetState
    {
        public LocalClient(IUserClient localUser) : base(localUser) { }

        public override bool IsConnected
        {
            get { return true; }
        }

        public void OnChat(Alienseed.BaseNetworkServer.INetState client, string text)
        {
            OnChat(client.User, text);
        }

        public void OnUserLogin(Alienseed.BaseNetworkServer.Accounting.IUserClient client)
        {
            OnUserConnected(client);
        }

        public void OnUserLogout(Alienseed.BaseNetworkServer.Accounting.IUserClient client)
        {
            OnUserDisconnected(client);
        }

        public void OnUserConnected(Alienseed.BaseNetworkServer.INetState client)
        {
            //throw new NotImplementedException();
        }

        public void OnUserDisconnected(Alienseed.BaseNetworkServer.INetState client)
        {
            //throw new NotImplementedException();
        }

        public override void Dispose()
        {

        }

        internal override void SendChat(string text)
        {
            Server.EventSink.OnChat(this, text);
        }

        internal override void SendLoginInfo()
        {
            Server.EventSink.OnLogin(LocalUser);
        }

        IUserClient IClient.User
        {
            get { return LocalUser; }
        }

        bool IClient.Connected
        {
            get { return this.IsConnected; }
        }
    }
}
