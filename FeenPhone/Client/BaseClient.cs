using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone.Client
{
    abstract class BaseClient : IDisposable
    {
        protected readonly IUserClient LocalUser;

        public BaseClient(IUserClient localUser)
        {
            this.LocalUser = localUser;
        }

        public abstract bool IsConnected { get; }

        public void OnChat(IUser user, string text)
        {
            EventSource.InvokeOnChat(this, user, text);
        }

        public void OnUserConnected(IUser user)
        {
            EventSource.InvokeOnUserConnected(this, user);
        }

        public void OnUserDisconnected(IUser user)
        {
            EventSource.InvokeOnUserDisconnected(this, user);
        }

        public abstract void Dispose();

        internal abstract void SendChat(string text);
        internal abstract void SendLoginInfo();
    }
}
