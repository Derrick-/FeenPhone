using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace Alienseed.BaseNetworkServer
{
    public class DataReadEventArgs : EventArgs
    {
        public Queue<byte> data { get; set; }
        public DataReadEventArgs(Queue<byte> data)
        {
            this.data = data;
        }
    }

    public abstract class BaseNetworkReader : INetworkReader
    {
        public event OnDisconnectHandler OnDisconnect;
        public event EventHandler<BufferOverflowArgs> OnBufferOverflow;

        public event PreviewIncomingHandler PreviewIncoming;

        public abstract void Dispose();

        protected internal bool InvokePreviewIncoming(ref byte[] bytes, ref int numbytes)
        {
            bool handled = false;
            if (PreviewIncoming != null)
                PreviewIncoming(ref bytes, ref numbytes, ref handled);
            return !handled;
        }

        protected internal void InvokeOnDisconnect()
        {
            if (OnDisconnect != null)
                OnDisconnect();
        }

        protected internal bool InvokeOnBufferOverflow()
        {
            bool handled = false;
            if (OnBufferOverflow != null)
            {
                var args = new BufferOverflowArgs();
                OnBufferOverflow(this, args);
                handled = args.handled;
            }
            return handled;
        }

        protected abstract void OnRead();


    }

    public class BaseUDPReader : BaseNetworkReader
    {
        public BaseUDPReader() { }

        public event EventHandler<DataReadEventArgs> OnReadData;

        protected Queue<byte> InStream = new Queue<byte>();

        private object readLock = new object();
        public void ReceivedData(byte[] data)
        {
            lock (readLock)
            {
                foreach (byte b in data)
                    InStream.Enqueue(b);
                OnRead();
            }
        }

        protected override void OnRead()
        {
            if (OnReadData != null)
            {
                OnReadData(this, new DataReadEventArgs(InStream));
            }
        }

        public override void Dispose()
        {
        }

    }

    public abstract class BaseStreamReader : BaseNetworkReader
    {
        private byte[] _buffer;

        const int MaxQueueLength = ushort.MaxValue;

        public Stream Stream { get; private set; }

        protected Queue<byte> InStream = new Queue<byte>();

        public void SetStream(Stream stream, int buffersize = 255)
        {
            if (_buffer == null || _buffer.Length != buffersize)
                _buffer = new byte[buffersize];

            Stream = stream;
            BeginRead();
        }

        private void BeginRead()
        {
            if (Stream != null && Stream.CanRead)
                try
                {
                    Stream.BeginRead(_buffer, 0, _buffer.Length, OnRead, Stream);
                }
                catch (NetworkException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            else
                if (!IsDisposed)
                    InvokeOnDisconnect();
        }

        private void OnRead(IAsyncResult ar)
        {
            Stream stream = ar.AsyncState as Stream;
            if (stream != null && stream == Stream)
            {
                int read = 0;
                try
                {
                    try
                    {
                        read = stream.EndRead(ar);
                    }
                    catch (IOException) { read = 0; }
                    catch (ObjectDisposedException) { read = 0; }
                    if (read == 0)
                    {
                        InvokeOnDisconnect();
                        return;
                    }
                }
                catch (IOException)
                {
                    InvokeOnDisconnect();
                    return;
                }

                if (InStream.Count + read > MaxQueueLength)
                {
                    bool handled = InvokeOnBufferOverflow();
                    if (!handled)
                        throw new IncomingBufferOverflowException();
                    else
                        InStream.Clear();
                }

                if (InvokePreviewIncoming(ref _buffer, ref read))
                {
                    for (int i = 0; i < read; i++)
                        InStream.Enqueue(_buffer[i]);
                    OnRead();
                }

                BeginRead();
            }
        }

        public bool IsDisposed { get; private set; }
        public override void Dispose()
        {
            Stream = null;
            IsDisposed = true;
        }
    }
}
