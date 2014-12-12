using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone.Client
{
    abstract class BaseClient : IDisposable
    {
        public abstract bool IsConnected { get; }

        public void OnChat(IUser user, string text)
        {
            EventSink.InvokeOnChat(this, user, text);
        }

        public void OnUserConnected(IUser user)
        {
            EventSink.InvokeOnUserConnected(this, user);
        }

        public void OnUserDisconnected(IUser user)
        {
            EventSink.InvokeOnUserDisconnected(this, user);
        }

        public abstract void Dispose();

        internal abstract void SendChat(string text);
    }
}
