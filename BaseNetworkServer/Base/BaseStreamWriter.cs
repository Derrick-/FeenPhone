using System.IO;

namespace Alienseed.BaseNetworkServer
{
    public abstract class BaseStreamWriter : INetworkWriter
    {
        public Stream Stream { get; private set; }

        public void SetStream(Stream stream)
        {
            Stream = stream;
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
