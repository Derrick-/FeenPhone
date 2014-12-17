using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone.Client
{
    internal static class UserRepo
    {

        public class UserInfo : IUser
        {
            public string Username { get; set; }
            public string Nickname { get; set; }
            public bool IsAdmin { get; set; }
            public Guid ID { get; set; }
            public bool Equals(IUser other)
            {
                return other.ID == ID;
            }
        }

        static Dictionary<Guid, UserInfo> KnownUsers = new Dictionary<Guid, UserInfo>();

        internal static IUser CreateOrUpdateUser(Guid id, bool isadmin, string username, string nickname)
        {
            UserInfo user;
            lock (KnownUsers)
            {
                if (KnownUsers.ContainsKey(id))
                    user = KnownUsers[id];
                else
                    KnownUsers.Add(id, user = new UserInfo() { ID = id });
            }

            user.IsAdmin = isadmin;
            user.Username = username;
            user.Nickname = nickname;

            return user;
        }
    }
}
