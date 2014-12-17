using System;

namespace Alienseed.BaseNetworkServer
{
    public delegate void OnDisconnectHandler();
    public delegate void PreviewIncomingHandler(ref byte[] bytes, ref int numbytes, ref bool handled);

    public class BufferOverflowArgs : EventArgs
    {
        public bool handled { get; set; }
    }

    public interface INetworkReader : IDisposable
    {
        event EventHandler<BufferOverflowArgs> OnBufferOverflow;
        event OnDisconnectHandler OnDisconnect;
        event PreviewIncomingHandler PreviewIncoming;
    }
}
