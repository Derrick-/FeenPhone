using Alienseed.BaseNetworkServer.Accounting;
using Alienseed.BaseNetworkServer.Network;
using Alienseed.BaseNetworkServer.Network.Telnet;
using FeenPhone.Server.Telnet.Prompts;
using System;
using System.IO;
using System.Net;

namespace FeenPhone.Server.Telnet
{
    class TelNetState : BaseTelNetState, IFeenPhoneNetstate
    {
        public TelNetState(Stream stream, IPEndPoint ep) : base(stream, ep) { }

        public override string WelcomeMessage
        {
            get { return "Welcome to FeenPhone!"; }
        }
        public override Alienseed.BaseNetworkServer.Network.Telnet.Prompts.BaseTextPrompt CreateFirstPrompt()
        {
            return new LoginPrompt();
        }

        #region ConnectionState

        internal bool Login(string Username, string password)
        {
            var user = FeenPhone.Accounting.AccountHandler.Login(Username, password);

            LogLine("Login {0}: {1}", user != null ? "SUCCESS" : "FAILED", user != null ? user.Username : Username);

            if (user == null) return false;

            return LoginSetUser(user);
        }

        #endregion

        public override void Dispose()
        {
            base.Dispose();
        }

        public void OnUserConnected(NetState user)
        {
            SendInfoLine("  {0} connected.", NicknameOrIP(user));
        }

        public void OnUserDisconnected(NetState user)
        {
            SendInfoLine("  {0} disconnected.", NicknameOrIP(user));
        }

        public void OnChat(NetState user, string text)
        {
            string line = string.Format("  {0} says: {1}", NicknameOrIP(user), text);
            SendInfoLine(line);
        }

        public void OnUserLogin(IUserClient client)
        {
            SendInfoLine("  {0} login.", client.Nickname);
        }

        public void OnUserLogout(IUserClient client)
        {
            SendInfoLine("  {0} logout.", client.Nickname);
        }

        private static string NicknameOrIP(NetState user)
        {
            return user.User != null ? user.User.Nickname : user.EndPoint.ToString();
        }
    }
}
