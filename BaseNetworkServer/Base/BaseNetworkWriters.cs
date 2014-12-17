using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Alienseed.BaseNetworkServer
{
    public abstract class BaseNetworkWriter : INetworkWriter
    {
        public abstract void Dispose();

        public abstract void Write(byte[] bytes);
    }

    public class BaseUDPWriter : BaseNetworkWriter
    {
        IPEndPoint ep;

        Socket socket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        public BaseUDPWriter() { }

        internal void SetEndpoint(IPEndPoint ep)
        {
            this.ep = ep;
        }

        public override void Write(byte[] bytes)
        {
            socket.SendTo(bytes, ep);
        }

        public override void Dispose()
        {
            socket.Dispose();
        }
    }

    public abstract class BaseStreamWriter : BaseNetworkWriter
    {
        int DefaultStreamWriteTimeout = 50;

        public Stream Stream { get; private set; }

        public void SetStream(Stream stream)
        {
            Stream = stream;
            if (stream != null && stream.WriteTimeout < 0)
                Stream.WriteTimeout = DefaultStreamWriteTimeout;
        }

        public override void Write(byte[] bytes)
        {
            Write(bytes, bytes.Length);
        }

        internal void Write(byte[] bytes, int numbytes)
        {
            if (Stream != null)
            {
                try
                {
                    Stream.Write(bytes, 0, numbytes);
                }
                catch (IOException)
                {
                }
            }
        }

        public override void Dispose()
        {
            Stream = null;
        }
    }
}
