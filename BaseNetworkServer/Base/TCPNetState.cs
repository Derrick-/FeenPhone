using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace  Alienseed.BaseNetworkServer
{
    public abstract class TCPNetState<TReader, TWriter> : NetState, IDisposable
        where TReader : BaseStreamReader, new()
        where TWriter : BaseStreamWriter, new()
    {
        public TReader Reader { get; private set; }
        public TWriter Writer { get; private set; }

        protected override string ClientIdentifier { get { return string.Format("{0} {1}", Address.ToString(), User != null ? User.Username : "NULL"); } }
        
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

        private Stream Stream { get; set; }

        internal TCPNetState(Stream stream, IPEndPoint ep)
            : base(ep)
        {
            Stream = stream;

            Reader = new TReader(); Reader.SetStream(stream);
            Writer = new TWriter(); Writer.SetStream(stream);

            Reader.OnDisconnect += Dispose;

            OnConnected();
        }

        public sealed override void Write(byte[] bytes)
        {
            Writer.Write(bytes);
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
