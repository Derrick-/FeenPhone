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
        UdpClient client;

        public BaseUDPWriter() { }

        public void SetClient(UdpClient client)
        {
            this.client = client;
        }

        public override void Write(byte[] bytes)
        {
            if (client != null)
            {
                if (client.Client == null)
                {
                    Dispose();
                }
                else
                {
                    if (client.Client.Connected)
                        client.Send(bytes, bytes.Length);
                    else
                        client.BeginSend(bytes, bytes.Length, new AsyncCallback(EndSend), null);
                }
            }
        }

        private void EndSend(IAsyncResult ar)
        {
            if (client != null)
            {
                int sent=client.EndSend(ar);
            }
        }

        public override void Dispose()
        {
            if (client != null)
            {
                client = null;
            }
        }
    }

    public abstract class BaseStreamWriter : BaseNetworkWriter
    {
        int DefaultStreamWriteTimeout = 50;

        public Stream Stream { get; private set; }

        public void SetStream(Stream stream)
        {
            Stream = stream;
            if (stream is NetworkStream && stream.WriteTimeout < 0)
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
