using Alienseed.BaseNetworkServer.Accounting;
using FeenPhone.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone.Server.TcpPacketServer
{

    class ServerPacketHandler : BasePacketHandler
    {
        private readonly TcpPacketNetState state;

        public ServerPacketHandler(TcpPacketNetState state)
        {
            this.state = state;
        }

        protected override void UserLogin(IUser user)
        {
            Console.WriteLine("Invalid server packet UserLogin received.");
        }

        protected override void UserLogout(IUser user)
        {
            Console.WriteLine("Invalid server packet UserLogout received.");
        }

        protected override void OnChat(IUser user, string text)
        {
            EventSink.OnChat(state, text);
        }

        protected override void OnLoginStatus(bool isLoggedIn)
        {
            Console.WriteLine("Invalid server packet LoginDemand received.");
        }

        protected override void LoginInfo(string username, string password)
        {
            if (state.Login(username, password))
                state.OnLoginSuccess();
            else
                state.OnLoginFailed();

        }

        protected override void UserList(IEnumerable<IUser> users)
        {
            Console.WriteLine("Invalid server packet UserList received.");
        }

        protected override IUser GetUserObject(Guid id, bool isadmin, string username, string nickname)
        {
            return AccountHandler.FindUser(id);
        }
    }
}
