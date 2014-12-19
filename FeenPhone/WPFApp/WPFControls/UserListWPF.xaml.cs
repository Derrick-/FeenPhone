using Alienseed.BaseNetworkServer.Accounting;
using FeenPhone.Client;
using FeenPhone.WPFApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FeenPhone.WPFApp.Controls
{
    /// <summary>
    /// Interaction logic for UserListWPF.xaml
    /// </summary>
    public partial class UserListWPF : UserControl
    {
        public ObservableCollection<UserStatusModel> Users = new ObservableCollection<UserStatusModel>();

        public UserListWPF()
        {
            InitializeComponent();

            EventSource.OnUserConnected += EventSource_OnConnected;
            EventSource.OnUserDisconnected += EventSource_OnDisconnected;

            EventSource.OnUserList += EventSource_OnUserList;

            UsersList.ItemsSource = Users;
        }

        private void EventSource_OnConnected(object sender, OnUserEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action<IUser>((user) => OnConnected(user)), e.User);
        }

        private void EventSource_OnDisconnected(object sender, OnUserEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action<IUser>((user) => OnDisconnected(user)), e.User);
        }
     
        void EventSource_OnUserList(object sender, UserListEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action<IEnumerable<IUser>>((users) => OnUserList(users)), e.Users);
        }

        void OnUserList(IEnumerable<IUser> users)
        {
            Users.Clear();
            if (users != null)
                foreach (var user in users)
                    Users.Add(new UserStatusModel(user));
        }

        private void OnConnected(IUser user)
        {
            if (!Users.Any(m => m.ID == user.ID))
                Users.Add(new UserStatusModel(user));
        }

        private void OnDisconnected(IUser user)
        {
            if (Users.Any(m => m.ID == user.ID))
                Users.Remove(Users.Single(m => m.ID == user.ID));
        }
    }
}
