using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace Alienseed.BaseNetworkServer
{
    public abstract class BaseStreamReader : INetworkReader
    {
        public event OnDisconnectHandler OnDisconnect;
        public event EventHandler<BufferOverflowArgs> OnBufferOverflow;

        public delegate void PreviewIncomingHandler(ref byte[] bytes, ref int numbytes);
        public event PreviewIncomingHandler PreviewIncoming;

        const int MaxQueueLength = short.MaxValue;

        public Stream Stream { get; private set; }

        protected Queue<byte> InStream = new Queue<byte>();

        public void SetStream(Stream stream)
        {
            Stream = stream;
            BeginRead();
        }

        private void BeginRead()
        {
            if (Stream != null && Stream.CanRead)
                try
                {
                    Stream.BeginRead(_buffer, 0, 255, OnRead, Stream);
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

        private byte[] _buffer = new byte[255];
        private void OnRead(IAsyncResult ar)
        {
            NetworkStream stream = ar.AsyncState as NetworkStream;
            if (stream != null && stream == Stream)
            {
                int read = 0;
                try
                {
                    try
                    {
                        read = stream.EndRead(ar);
                    }
                    catch (ObjectDisposedException)
                    {
                        read = 0;
                    }
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
                    bool handled = false;
                    if (OnBufferOverflow != null)
                    {
                        var args = new BufferOverflowArgs();
                        OnBufferOverflow(this, args);
                        handled = args.handled;
                    }
                    if (!handled)
                        throw new IncomingBufferOverflowException();
                    else
                        InStream.Clear();
                }

                InvokePreviewIncoming(ref _buffer, ref read);

                for (int i = 0; i < read; i++)
                    InStream.Enqueue(_buffer[i]);
                OnRead();

                BeginRead();
            }
        }

        void InvokePreviewIncoming(ref byte[] bytes, ref int numbytes)
        {
            if (PreviewIncoming != null)
                PreviewIncoming(ref bytes, ref numbytes);
        }

        void InvokeOnDisconnect()
        {
            if (OnDisconnect != null)
                OnDisconnect();
        }

        protected abstract void OnRead();

        public bool IsDisposed { get; private set; }
        public void Dispose()
        {
            Stream = null;
            IsDisposed = true;
        }
    }
}
