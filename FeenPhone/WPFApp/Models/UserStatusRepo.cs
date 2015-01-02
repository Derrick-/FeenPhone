using Alienseed.BaseNetworkServer.Accounting;
using FeenPhone.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeenPhone.WPFApp.Models
{
    internal static class UserStatusRepo
    {
        public static ObservableCollection<UserStatusModel> Users = new ObservableCollection<UserStatusModel>();

        static UserStatusRepo()
        {
            EventSource.OnUserConnected += EventSource_OnConnected;
            EventSource.OnUserDisconnected += EventSource_OnDisconnected;

            EventSource.OnUserList += EventSource_OnUserList;
        }

        public static UserStatusModel FindUser(Guid userID)
        {
            return Users.SingleOrDefault(m => m.ID == userID);
        }

        private static void EventSource_OnConnected(object sender, OnUserEventArgs e)
        {
            Dispatch(new Action<IUser>((user) => OnConnected(user)), e.User);
        }

        private static void EventSource_OnDisconnected(object sender, OnUserEventArgs e)
        {
            Dispatch(new Action<IUser>((user) => OnDisconnected(user)), e.User);
        }

        private static void EventSource_OnUserList(object sender, UserListEventArgs e)
        {
            Dispatch(new Action<IEnumerable<IUser>>((users) => OnUserList(users)), e.Users);
        }

        private static void Dispatch<T>(Action<T> a, params object[] args)
        {
            App.Current.Dispatcher.BeginInvoke(a, args);
        }

        private static void OnUserList(IEnumerable<IUser> users)
        {
            Users.Clear();
            if (users != null)
                foreach (var user in users)
                    Users.Add(new UserStatusModel(user));
        }

        private static void OnConnected(IUser user)
        {
            if (!Users.Any(m => m.ID == user.ID))
                Users.Add(new UserStatusModel(user));
        }

        private static void OnDisconnected(IUser user)
        {
            if (Users.Any(m => m.ID == user.ID))
                Users.Remove(Users.Single(m => m.ID == user.ID));
        }

    }
}
