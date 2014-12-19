using Alienseed.BaseNetworkServer.Accounting;
using FeenPhone.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FeenPhone.WPFApp.Models
{
    public class UserStatusModel : DependencyObject, IUser
    {
        private readonly IUser User;
        private readonly IFeenPhoneNetState State;

        public UserStatusModel(IUser user)
        {
            this.User = user;

            if (User is IUserClient)
            {
                var client = ((IUserClient)User).Client as IFeenPhoneNetState;
                if (client != null)
                {
                    client.PropertyChanged += client_PropertyChanged;
                    this.State = client;
                }
            }
        }

        void client_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action<PropertyChangedEventArgs>(HandleClientPropertyChanged), e);
        }

        void HandleClientPropertyChanged(PropertyChangedEventArgs e)
        {
            if (State != null)
            {
                if (e.PropertyName == "LastPing") { LastPing = State.LastPing; }
            }
        }

        public static DependencyProperty LastPingProperty = DependencyProperty.Register("LastPing", typeof(int?), typeof(UserStatusModel), new PropertyMetadata(null));
        public int? LastPing
        {
            get { return (int)this.GetValue(LastPingProperty); }
            set { this.SetValue(LastPingProperty, value); }
        }

        public Guid ID { get { return User.ID; } }
        public string Username { get { return User.Username; } }
        public string Nickname { get { return User.Nickname; } }
        public bool IsAdmin { get { return User.IsAdmin; } }

        public bool Equals(IUser other)
        {
            return User.Equals(other);
        }


    }
}
