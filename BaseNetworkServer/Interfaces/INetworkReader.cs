using System;

namespace Alienseed.BaseNetworkServer.Network
{
    public delegate void OnDisconnectHandler();

    interface INetworkReader : IDisposable
    {
    }
}
