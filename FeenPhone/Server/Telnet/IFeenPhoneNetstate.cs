using Alienseed.BaseNetworkServer.Accounting;
using Alienseed.BaseNetworkServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace FeenPhone.Server
{
    interface IFeenPhoneNetstate
    {
        void OnUserConnected(NetState user);
        void OnUserDisconnected(NetState user);

        void OnUserLogin(IUserClient client);
        void OnUserLogout(IUserClient client);

        void OnChat(NetState user, string text);
    }
}