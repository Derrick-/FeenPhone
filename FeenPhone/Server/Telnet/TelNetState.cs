using Alienseed.BaseNetworkServer.Accounting;
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

        public void OnUserConnected(IUser user)
        {
            SendInfoLine("  {0} connected.", user);
        }

        public void OnUserDisconnected(IUser user)
        {
            SendInfoLine("  {0} disconnected.", user);
        }

        public void OnChat(IUser user, string text)
        {
            string line=string.Format("  {0} says: {1}", user.Nickname, text);
            SendInfoLine(line);
        }
    }
}
