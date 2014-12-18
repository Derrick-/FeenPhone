using System;

namespace Alienseed.BaseNetworkServer
{
    public interface INetworkWriter : IDisposable
    {
        void Write(byte[] bytes);
#if NET45
        System.Threading.Tasks.Task WriteAsync(byte[] bytes);
#endif
    }
}
