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
#if NET45
        public abstract System.Threading.Tasks.Task WriteAsync(byte[] bytes);
#endif
    }

    public class BaseUDPWriter : BaseNetworkWriter
    {
        UdpClient client;

        public BaseUDPWriter() { }

        public void SetClient(UdpClient client)
        {
            this.client = client;
        }

#if NET45
        public override System.Threading.Tasks.Task WriteAsync(byte[] bytes)
        {
            return System.Threading.Tasks.Task.Run(() => Write(bytes));
        }
#endif
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
                int sent = client.EndSend(ar);
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
        public Socket Socket { get; private set; }

        public void SetStream(Stream stream)
        {
            Stream = stream;
            if (stream is NetworkStream && stream.WriteTimeout < 0)
                Stream.WriteTimeout = DefaultStreamWriteTimeout;
        }

        internal void SetSocket(System.Net.Sockets.Socket socket)
        {
            Socket = socket;
        }

#if NET45
        public override async System.Threading.Tasks.Task WriteAsync(byte[] bytes)
        {
            await WriteAsync(bytes, bytes.Length);
        }

        internal async System.Threading.Tasks.Task WriteAsync(byte[] bytes, int numbytes)
        {
            try
            {
                if (Socket != null)
                    WriteDirect(bytes);
                else if (Stream != null)
                {
                    await Stream.WriteAsync(bytes, 0, numbytes);
                }
            }
            catch (IOException)
            {
            }
        }
#endif

        public override void Write(byte[] bytes)
        {
            Write(bytes, bytes.Length);
        }

        internal void Write(byte[] bytes, int numbytes)
        {
            try
            {
                if (Socket != null)
                    WriteDirect(bytes);
                else if (Stream != null)
                {
                    Stream.Write(bytes, 0, numbytes);
                }
            }
            catch (IOException)
            {
            }
        }

        private void WriteDirect(byte[] bytes)
        {
            Socket.Send(bytes);
        }

        public override void Dispose()
        {
            Stream = null;
            Socket = null;
        }
    }
}
