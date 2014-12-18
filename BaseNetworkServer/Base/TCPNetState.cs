using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Alienseed.BaseNetworkServer
{
    public abstract class TCPNetState<TReader, TWriter> : NetState, IDisposable
        where TReader : BaseStreamReader, new()
        where TWriter : BaseStreamWriter, new()
    {
        public TReader Reader { get; private set; }
        public TWriter Writer { get; private set; }

        protected System.Net.Sockets.Socket Socket;
        internal void SetSocket(System.Net.Sockets.Socket socket)
        {
            Socket=socket;
            if (Writer != null)
                Writer.SetSocket(socket);
        }

        protected override string ClientIdentifier { get { return string.Format("TCP {0} {1}", Address.ToString(), User != null ? User.Username : "TCP NULL"); } }

        public IPEndPoint IPEndPoint { get { return EndPoint as IPEndPoint; } }
        public IPAddress Address
        {
            get
            {
                if (IPEndPoint == null)
                    return IPAddress.None;
                return IPEndPoint.Address;
            }
        }

        private System.Net.Sockets.NetworkStream Stream { get; set; }

        internal TCPNetState(System.Net.Sockets.NetworkStream stream, IPEndPoint ep, int readBufferSize)
            : base(ep)
        {
            Stream = stream;

            Reader = new TReader(); Reader.SetStream(stream, readBufferSize);
            Writer = new TWriter(); Writer.SetStream(stream);

            Reader.OnDisconnect += Dispose;
            Reader.OnBufferOverflow += Reader_OnBufferOverflow;

            OnConnected();
        }

        protected abstract void Reader_OnBufferOverflow(object sender, BufferOverflowArgs e);

        public sealed override void Write(byte[] bytes)
        {
            if (Socket != null)
                WriteDirect(bytes);
            else
                Writer.Write(bytes);
        }

        public void WriteDirect(byte[] bytes)
        {
            Socket.Send(bytes);
        }

        protected virtual void OnConnected() { }

        public override void Dispose()
        {
            base.Dispose();

            Reader.Dispose();
            Writer.Dispose();
            Stream.Dispose();
        }
    }

}
