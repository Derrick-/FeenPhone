
using System;
namespace Alienseed.BaseNetworkServer
{
    public interface INetState : Alienseed.BaseNetworkServer.Accounting.IClient, IDisposable
    {
        bool LoginSetUser(Alienseed.BaseNetworkServer.Accounting.IUserClient user, bool dcIfLoggedIn);
        void LogLine(string format, object arg0);
        void LogLine(string format, params object[] args);
    }
}
