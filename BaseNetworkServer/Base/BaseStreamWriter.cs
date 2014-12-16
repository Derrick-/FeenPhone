using System.IO;

namespace Alienseed.BaseNetworkServer
{
    public abstract class BaseStreamWriter : INetworkWriter
    {
        int DefaultStreamWriteTimeout = 50;

        public Stream Stream { get; private set; }

        public void SetStream(Stream stream)
        {
            Stream = stream;
            if (stream != null && stream.WriteTimeout < 0)
                Stream.WriteTimeout = DefaultStreamWriteTimeout;
        }

        public void Write(byte[] bytes)
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

        public void Dispose()
        {
            Stream = null;
        }
    }
}
