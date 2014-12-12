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

            EventSink.OnUserConnected += this.OnConnected;
            EventSink.OnUserDisconnected += this.OnDisconnected;

            UsersList.ItemsSource = Users;
        }

        private void OnConnected(object sender, OnUserEventArgs e)
        {
            if (!Users.Any(m => m.ID == e.User.ID))
                Dispatcher.Invoke(new Action<IUser>(Users.Add), e.User);
        }

        private void OnDisconnected(object sender, OnUserEventArgs e)
        {
            Dispatcher.Invoke(new Action<IUser>(RemoveUser), e.User);
        }

        private void RemoveUser(IUser user)
        {
            if (Users.Any(m => m.ID == user.ID))
                Users.Remove(Users.Single(m => m.ID == user.ID));
        }

    }
}
