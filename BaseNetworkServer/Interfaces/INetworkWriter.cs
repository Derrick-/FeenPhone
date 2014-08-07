using System;

namespace  Alienseed.BaseNetworkServer
{
    interface INetworkWriter : IDisposable
    {
        void Write(byte[] bytes);
    }
}
