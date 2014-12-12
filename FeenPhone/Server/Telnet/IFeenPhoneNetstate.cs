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
        void OnUserConnected(INetState user);
        void OnUserDisconnected(INetState user);

        void OnUserLogin(IUserClient client);
        void OnUserLogout(IUserClient client);

        void OnChat(INetState user, string text);
    }
}