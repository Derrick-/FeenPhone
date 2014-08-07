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

        private Stream Stream { get; set; }

        internal TCPNetState(Stream stream, EndPoint ep)
            : base(ep)
        {
            Stream = stream;

            Reader = new TReader(); Reader.SetStream(stream);
            Writer = new TWriter(); Writer.SetStream(stream);

            Reader.OnDisconnect += Dispose;

            OnConnected();
        }

        protected abstract void OnConnected();

        public override void Dispose()
        {
            base.Dispose();

            Reader.Dispose();
            Writer.Dispose();
            Stream.Dispose();
        }
    }

}
