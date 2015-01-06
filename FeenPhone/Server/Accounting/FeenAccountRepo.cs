using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FeenPhone.Accounting
{

    class PasswordOnlyRepo : AutoCreateRepo
    {
        public static string RequirePassword { get; set; }
        public override ushort Version { get { return RequirePassword == null ? base.Version : (ushort)1; } }

        public override string RequestLoginMessage { get { return RequirePassword != null ? "Login with unique username. Password is required" : base.RequestLoginMessage; } }
        public override string LoginSuccessMessage { get { return "Login success."; } }
        public override string LoginFailureMessage { get { return RequirePassword != null ? "Username in use or incorrect password." : base.LoginFailureMessage; } }

        public override IUserClient Login(string username, string password)
        {
            if (RequirePassword != null && password != RequirePassword)
                return null;

            return base.Login(username, password);
        }
    }

    class AutoCreateRepo : BaseFeenAccountRepo
    {
        public override ushort Version { get { return 0; } }
        public AutoCreateRepo() : base(true) { }

        public override string RequestLoginMessage { get { return "Login with unique username."; } }
        public override string LoginSuccessMessage { get { return "Login success."; } }
        public override string LoginFailureMessage { get { return "Username in use."; } }
    }

    abstract class BaseFeenAccountRepo : IAccountRepository
    {
        public abstract ushort Version { get; }
        public abstract string RequestLoginMessage { get; }
        public abstract string LoginSuccessMessage { get; }
        public abstract string LoginFailureMessage { get; }

        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

        public bool AutoCreateAccounts { get; set; }

        public BaseFeenAccountRepo(bool autoCreate = false)
        {
            AutoCreateAccounts = autoCreate;
        }

        static readonly Dictionary<string, IUserClient> accounts = new Dictionary<string, IUserClient>();

        #region IAccountRepository Members

        public virtual IUserClient Login(string username, string password)
        {
            IUserClient user;

            if (!string.IsNullOrWhiteSpace(username))
            {
                if (accounts.TryGetValue(username.ToLowerInvariant(), out user))
                    return user;

                if (AutoCreateAccounts)
                {
                    var newAccount = new MockAccount(username.ToLowerInvariant(), textInfo.ToTitleCase(username));
                    accounts.Add(newAccount.Username, newAccount);
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
