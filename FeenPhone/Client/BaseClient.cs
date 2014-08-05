using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone.Client
{
    public class OnUserEventArgs : EventArgs
    {
        public IUser User { get; private set; }
        public OnUserEventArgs(IUser user)
        {
            User = user;
        }
    }

    public class OnChatEventArgs : EventArgs
    {
        public IUser User { get; private set; }
        public string Text { get; private set; }
        public OnChatEventArgs(IUser user, string text)
        {
            User = user;
            Text = text;
        }
    }

    abstract class BaseClient
    {
        public abstract bool IsConnected { get; }

        public event EventHandler<OnUserEventArgs> OnUserConnected;
        public event EventHandler<OnUserEventArgs> OnUserDisconnected;
        public event EventHandler<OnChatEventArgs> OnChat;

        public void InvokeOnUserConnected(IUser user)
        {
            if (OnUserConnected != null)
                OnUserConnected(this, new OnUserEventArgs(user));
        }

        public void InvokeOnUserDisconnected(IUser user)
        {
            if (OnUserDisconnected != null)
                OnUserDisconnected(this, new OnUserEventArgs(user));
        }

        public void InvokeOnChat(IUser user, string text)
        {
            if (OnChat != null)
                OnChat(this, new OnChatEventArgs(user, text));
        }

    }
}
