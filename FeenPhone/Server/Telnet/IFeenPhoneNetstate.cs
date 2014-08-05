using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone.Server
{
    interface IFeenPhoneNetstate
    {
        void OnUserConnected(IUser user);
        void OnUserDisconnected(IUser user);
        void OnChat(IUser user, string text);

    }
}