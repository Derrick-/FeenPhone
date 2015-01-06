
using Alienseed.BaseNetworkServer.Accounting;
using System;

namespace FeenPhone.Accounting
{

    internal static class AccountHandler
    {
        public static AccountHandlerInstance<PasswordOnlyRepo> Instance = new AccountHandlerInstance<PasswordOnlyRepo>();
    }

    internal class AccountHandlerInstance<TRepo> where TRepo : IAccountRepository, new()
    {
        IAccountRepository _repo = null;
        IAccountRepository Repo { get { return _repo ?? (_repo = CreateRepo()); } }

        private static IAccountRepository CreateRepo()
        {
            return new TRepo();
        }

        public IUserClient Login(string username, string password)
        {
            return Repo.Login(username, password);
        }

        public IUser FindUser(Guid id)
        {
            return Repo.FindUser(id);
        }

        public ushort Version { get { return Repo.Version; } }
    }
}
