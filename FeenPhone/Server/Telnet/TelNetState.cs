using Alienseed.BaseNetworkServer.Accounting;
using Alienseed.BaseNetworkServer.Network.Telnet.Prompts;
using System.IO;
using System.Net;

namespace Alienseed.BaseNetworkServer.Network.Telnet
{
    class TelNetState : BaseTelNetState
    {
        public TelNetState(Stream stream, IPEndPoint ep) : base(stream, ep) { }

        public override string WelcomeMessage
        {
            get { return "Welcome to FeenPhone!"; }
        }
        public override BaseTextPrompt CreateFirstPrompt()
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

    }
}
