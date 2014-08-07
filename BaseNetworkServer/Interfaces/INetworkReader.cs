using System;

namespace  Alienseed.BaseNetworkServer
{
    public delegate void OnDisconnectHandler();

    interface INetworkReader : IDisposable
    {
    }
}
