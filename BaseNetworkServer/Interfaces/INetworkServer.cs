using System.Net;

namespace Alienseed.BaseNetworkServer.Network
{
    public delegate void OnListenerCrashHandler(INetworkServer server);

    public interface INetworkServer
    {
        IPAddress Address { get; }
        int Port { get; }
        bool Start();
        void Stop();
        bool Running { get; }

        event OnListenerCrashHandler OnListenerCrash;
    }
}
