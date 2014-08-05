using System;
using Alienseed.BaseNetworkServer.Accounting;

namespace FeenPhone.Accounting
{
    class Account : Alienseed.BaseNetworkServer.Accounting.IUserClient
    {
        public Guid ID { get { return providerID; } }
        internal Guid providerID { get; private set; }

        public string Username { get; private set; }
        public string Nickname { get; internal set; }

        public bool IsAdmin { get; private set; }

        public IClient Client { get; private set; }

        public bool SetClient(IClient client)
        {
            if (Client == client) return true;
            if ((Client == null) != (client == null))
            {
                Client = client;
                return true;
            }
            return false;
        }

        internal Account(Guid providerid, string username, string nickname = null, bool isadmin = false)
        {
            providerID = providerid;
            Username = username;
            Nickname = nickname ?? username;
            IsAdmin = isadmin;
        }

        public override bool Equals(object obj)
        {
            if (obj is IUser) return Equals((IUser)obj);
            return false;
        }

        public override int GetHashCode()
        {
            return providerID.GetHashCode();
        }

        #region IEquatable<IUser> Members

        public bool Equals(IUser other)
        {
            if(!(other is Account)) return false;
            return ((Account)other).providerID == this.providerID;
        }

        #endregion

    }
}