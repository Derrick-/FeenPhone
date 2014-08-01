using System;

namespace Alienseed.BaseNetworkServer.Network
{
    interface INetworkWriter : IDisposable
    {
        void Write(byte[] bytes);
    }
}
