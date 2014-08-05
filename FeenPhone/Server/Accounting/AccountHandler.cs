
using Alienseed.BaseNetworkServer.Accounting;
namespace FeenPhone.Accounting
{
    public static class AccountHandler
    {
        static readonly bool UseMockRepo = true;

        static IAccountRepository _repo = null;
        static IAccountRepository Repo { get { return _repo ?? (_repo = CreateRepo()); } }

        private static IAccountRepository CreateRepo()
        {
            if (UseMockRepo)
                return new MockRepo();
            return null; //new SQL.SqlAccountRepo();
        }

        public static IUserClient Login(string username, string password)
        {
            return Repo.Login(username, password);
        }
    }
}
