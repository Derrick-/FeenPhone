using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace Alienseed.BaseNetworkServer
{
    public abstract class BaseStreamReader : INetworkReader
    {
        private byte[] _buffer;

        public event OnDisconnectHandler OnDisconnect;
        public event EventHandler<BufferOverflowArgs> OnBufferOverflow;

        public delegate void PreviewIncomingHandler(ref byte[] bytes, ref int numbytes, ref bool handled);
        public event PreviewIncomingHandler PreviewIncoming;

        const int MaxQueueLength = ushort.MaxValue;

        public Stream Stream { get; private set; }

        protected Queue<byte> InStream = new Queue<byte>();

        public void SetStream(Stream stream, int buffersize=255)
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

                if (InvokePreviewIncoming(ref _buffer, ref read))
                {
                    for (int i = 0; i < read; i++)
                        InStream.Enqueue(_buffer[i]);
                    OnRead();
                }

                BeginRead();
            }
        }

        bool InvokePreviewIncoming(ref byte[] bytes, ref int numbytes)
        {
            bool handled = false;
            if (PreviewIncoming != null)
                PreviewIncoming(ref bytes, ref numbytes, ref handled);
            return !handled;
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
