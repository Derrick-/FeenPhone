using Alienseed.BaseNetworkServer.Accounting;
using Alienseed.BaseNetworkServer;
using Alienseed.BaseNetworkServer.Telnet;
using FeenPhone.Server.Telnet.Prompts;
using System;
using System.IO;
using System.Net;

namespace FeenPhone.Server.Telnet
{
    class TelNetState : BaseTelNetState, IFeenPhoneNetstate
    {
        public IFeenPhoneClientNotifier Notifier { get; private set; }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public TelNetState(System.Net.Sockets.NetworkStream stream, IPEndPoint ep) : base(stream, ep)
        {
            Notifier = new TelnetClientNotificationHandler(this);
        }

        protected override void Reader_OnBufferOverflow(object sender, BufferOverflowArgs e)
        {
            Console.WriteLine("Buffer overflow from {0}", this.ClientIdentifier);
            e.handled = true;
        }

        public override string WelcomeMessage
        {
            get { return "Welcome to FeenPhone!"; }
        }
        public override Alienseed.BaseNetworkServer.Telnet.Prompts.BaseTextPrompt CreateFirstPrompt()
        {
            return new LoginPrompt();
        }

        #region ConnectionState

        #endregion

        public override void Dispose()
        {
            base.Dispose();
        }

        public class TelnetClientNotificationHandler : IFeenPhoneClientNotifier
        {
            private TelNetState telNetState;

            public TelnetClientNotificationHandler(TelNetState telNetState)
            {
                this.telNetState = telNetState;
            }
     
            public void OnUserConnected(INetState user)
            {
                telNetState.SendInfoLine("  {0} connected.", NicknameOrIP(user));
            }

            public void OnUserDisconnected(INetState user)
            {
                telNetState.SendInfoLine("  {0} disconnected.", NicknameOrIP(user));
            }

            public void OnChat(INetState user, string text)
            {
                string line = string.Format("  {0} says: {1}", NicknameOrIP(user), text);
                telNetState.SendInfoLine(line);
            }

            public void OnUserLogin(IUserClient client)
            {
                telNetState.SendInfoLine("  {0} login.", client.Nickname);
            }

            public void OnUserLogout(IUserClient client)
            {
                telNetState.SendInfoLine("  {0} logout.", client.Nickname);
            }

            public void OnAudio(Guid userID, Audio.Codecs.CodecID Codec, byte[] data, int dataLen)
            {
                // nothing for telnet
            }

            public bool Login(string Username, string password)
            {
                var user = FeenPhone.Accounting.AccountHandler.Login(Username, password);

                telNetState.LogLine("Login {0}: {1}", user != null ? "SUCCESS" : "FAILED", user != null ? user.Username : Username);

                if (user == null) return false;

                return telNetState.LoginSetUser(user, false);
            }

            public void LoginSuccess()
            {
                telNetState.WriteLine("  Welcome.");
            }

            public void LoginFailed()
            {
                telNetState.WriteLine("  Login failed.");
            }
        }

        private static string NicknameOrIP(INetState user)
        {
            if (user.User != null)
                return user.User.Nickname;
            if (user is NetState)
                return ((NetState)user).EndPoint.ToString();
            return "Unknown";
        }

        public ushort LastPing { get { return 0; } set { InvokePropertyChanged("LastPing"); } }

        private void InvokePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propName));
        }
    }
}
