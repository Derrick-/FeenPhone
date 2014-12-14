using Alienseed.BaseNetworkServer.Accounting;
using FeenPhone.Client;
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

namespace FeenPhone.WPFControls
{
    /// <summary>
    /// Interaction logic for UserListWPF.xaml
    /// </summary>
    public partial class UserListWPF : UserControl
    {
        public ObservableCollection<IUser> Users = new ObservableCollection<IUser>();

        public UserListWPF()
        {
            InitializeComponent();

            EventSource.OnUserConnected += this.OnConnected;
            EventSource.OnUserDisconnected += this.OnDisconnected;

            EventSource.OnUserList += OnUserList;

            UsersList.ItemsSource = Users;
        }

        void OnUserList(object sender, UserListEventArgs e)
        {
            ClearUsers();
            if (e.Users != null)
                foreach (var user in e.Users)
                    AddUser(user);
        }

        private void ClearUsers()
        {
            Dispatcher.Invoke(new Action(Users.Clear));
        }

        private void OnConnected(object sender, OnUserEventArgs e)
        {
            if (!Users.Any(m => m.ID == e.User.ID))
                AddUser(e.User);
        }

        private void OnDisconnected(object sender, OnUserEventArgs e)
        {
            RemoveUser(e.User);
        }

        private void AddUser(IUser user)
        {
            Dispatcher.Invoke(new Action<IUser>(Users.Add), user);
        }

        private void RemoveUser(IUser user)
        {
            Dispatcher.Invoke(new Action<Guid>((id) =>
            {
                if (Users.Any(m => m.ID == user.ID))
                    Users.Remove(Users.Single(m => m.ID == id));
            }), user.ID);
        }

    }
}
