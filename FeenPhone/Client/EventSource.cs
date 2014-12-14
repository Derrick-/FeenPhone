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

    public class UserListEventArgs : EventArgs
    {
        public IEnumerable<IUser> Users { get; private set; }
        public UserListEventArgs(IEnumerable<IUser> users)
        {
            Users = users;
        }
    }

    public class BoolEventArgs : EventArgs
    {
        public bool Value { get; private set; }
        public BoolEventArgs(bool value)
        {
            Value = value;
        }
    }

    internal static class EventSource
    {
        public static event EventHandler<OnUserEventArgs> OnUserConnected;
        public static event EventHandler<OnUserEventArgs> OnUserDisconnected;
        public static event EventHandler<OnChatEventArgs> OnChat;
        public static event EventHandler<UserListEventArgs> OnUserList;
        public static event EventHandler<BoolEventArgs> OnLoginStatus;

        public static void InvokeOnUserConnected(object sender, IUser user)
        {
            if (OnUserConnected != null)
                OnUserConnected(sender, new OnUserEventArgs(user));
        }

        public static void InvokeOnUserDisconnected(object sender, IUser user)
        {
            if (OnUserDisconnected != null)
                OnUserDisconnected(sender, new OnUserEventArgs(user));
        }

        public static void InvokeOnChat(object sender, IUser user, string text)
        {
            if (OnChat != null)
                OnChat(sender, new OnChatEventArgs(user, text));
        }

        internal static void InvokeOnLoginStatus(object sender, bool isLoggedIn)
        {
            if (OnLoginStatus != null)
                OnLoginStatus(sender, new BoolEventArgs(isLoggedIn));
        }

        internal static void InvokeOnUserList(object sender, IEnumerable<IUser> users)
        {
            if (OnUserList != null)
                OnUserList(sender, new UserListEventArgs(users));
        }
    }
}
