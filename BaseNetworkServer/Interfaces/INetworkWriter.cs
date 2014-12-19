using System;

namespace Alienseed.BaseNetworkServer
{
    public interface INetworkWriter : IDisposable
    {
        void Write(byte[] bytes);
    }
}
