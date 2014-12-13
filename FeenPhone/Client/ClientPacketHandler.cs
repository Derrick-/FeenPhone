using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone.Client
{
    class ClientPacketHandler : BasePacketHandler
    {
        protected override void OnChat(IUser user, string text)
        {
            EventSource.InvokeOnChat(this, user, text);
        }

        protected override void OnLoginStatus(bool isLoggedIn)
        {
            EventSource.InvokeOnLoginStatus(this, isLoggedIn);
        }

        protected override void LoginInfo(string username, string password)
        {
            Console.WriteLine("Invalid client packet LoginInfo received.");
        }

        protected override IUser GetUserObject(Guid id, bool isadmin, string username, string nickname)
        {
            return UserRepo.CreateOrUpdateUser(id, isadmin, username, nickname);
        }
    }
}

