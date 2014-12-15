using System;

namespace Alienseed.BaseNetworkServer
{
    public delegate void OnDisconnectHandler();

    public class BufferOverflowArgs : EventArgs
    {
        public bool handled { get; set; }
    }

    interface INetworkReader : IDisposable
    {
    }
}
