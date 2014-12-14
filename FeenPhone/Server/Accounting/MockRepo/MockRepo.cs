using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FeenPhone.Accounting
{
    class MockRepo : IAccountRepository
    {
        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

        public bool AutoCreateAccounts { get; set; }

        public MockRepo(bool autoCreate = false)
        {
            AutoCreateAccounts = autoCreate;
        }

        static readonly Dictionary<string, IUserClient> accounts = new Dictionary<string, IUserClient>()
        {
            {"derrick",new MockAccount("derrick","Derrick",true)},
            {"mwd",new MockAccount("mwd","MWD",true)},
            {"ian",new MockAccount("ian","Hemperor",true)},
            {"donna",new MockAccount("donna","Donna",false)},
        };

        #region IAccountRepository Members

        public IUserClient Login(string username, string password)
        {
            IUserClient user;

            if (!string.IsNullOrWhiteSpace(username))
            {
                if (accounts.TryGetValue(username.ToLowerInvariant(), out user))
                    return user;

                if (AutoCreateAccounts)
                {
                    var newAccount = new MockAccount(username.ToLowerInvariant(), textInfo.ToTitleCase(username));
                    accounts.Add(username, newAccount);
                    return newAccount;
                }
            }
            return null;
        }

        public IUser FindUser(Guid id)
        {
            return accounts.SingleOrDefault(m => m.Value.ID == id).Value;
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
        }
    }
}
