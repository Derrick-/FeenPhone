using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone.Accounting
{
    class MockRepo : IAccountRepository
    {
        static readonly Dictionary<string, IUserClient> accounts = new Dictionary<string, IUserClient>()
        {
            {"derrick",new MockAccount("derrick","Derrick",true)},
            {"ian",new MockAccount("ian","Hemperor",true)},
            {"donna",new MockAccount("donna","donna",false)},
        };

        #region IAccountRepository Members

        public IUserClient Login(string username, string password)
        {
            IUserClient user;

            if (accounts.TryGetValue(username, out user))
                return user;
            return null;
        }

        #endregion
    }

    internal class MockAccount : Account
    {
        static int usernum = 0;

        public MockAccount() : this(null) { }

        public MockAccount(string username, string nickname = null, bool isadmin = false)
            : base(Guid.NewGuid(), username == null ? "user" + usernum++.ToString() : username, isadmin: isadmin)
        {
            if (nickname != null)
                Nickname = nickname;
        }
    }
}
