using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone.Client
{
    class LocalClient : BaseClient, FeenPhone.Server.IFeenPhoneNetstate
    {
        public LocalClient()
        {
        }

        public override bool IsConnected
        {
            get { return true; }
        }

        public void OnChat(Alienseed.BaseNetworkServer.NetState client, string text)
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

        public void OnUserConnected(Alienseed.BaseNetworkServer.NetState client)
        {
            //throw new NotImplementedException();
        }

        public void OnUserDisconnected(Alienseed.BaseNetworkServer.NetState client)
        {
            //throw new NotImplementedException();
        }

        public override void Dispose()
        {
            
        }

    }
}
