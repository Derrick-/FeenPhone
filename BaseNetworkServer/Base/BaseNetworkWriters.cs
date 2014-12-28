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
        private Stream Stream;

        public void SetStream(Stream stream)
        {
            Stream = stream;
        }

        public override void Write(byte[] bytes)
        {
            Write(bytes, bytes.Length);
        }

        internal void Write(byte[] bytes, int numbytes)
        {
            try
            {
                if (Stream != null && Stream.CanWrite)
                {
                    Stream.BeginWrite(bytes, 0, numbytes, new AsyncCallback(ConpleteWrite), this);
                }
            }
            catch (IOException)
            {
            }
        }

        private static void ConpleteWrite(IAsyncResult ar)
        {
            BaseStreamWriter writer = ar.AsyncState as BaseStreamWriter;
            if (writer != null && writer.Stream != null)
            {
                try
                {
                    writer.Stream.EndWrite(ar);
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
