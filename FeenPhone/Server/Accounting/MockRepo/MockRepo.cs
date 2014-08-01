using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alienseed.BaseNetworkServer.Accounting
{
    class MockRepo : IAccountRepository
    {
        static readonly Dictionary<string, IUserClient> accounts = new Dictionary<string, IUserClient>()
        {
            {"derrick",new Account(Guid.NewGuid(),"derrick","Derrick",true)},
            {"ian",new Account(Guid.NewGuid(),"ian","Hemperor",true)},
            {"donna",new Account(Guid.NewGuid(),"donna","donna",false)},
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
}
