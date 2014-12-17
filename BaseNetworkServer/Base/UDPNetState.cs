using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Alienseed.BaseNetworkServer
{
    public abstract class UDPNetState<TReader, TWriter> : NetState, IDisposable
        where TReader : BaseUDPReader, new()
        where TWriter : BaseUDPWriter, new()
    {
        public TReader Reader { get; private set; }
        public TWriter Writer { get; private set; }

        protected override string ClientIdentifier { get { return string.Format("UDP {0} {1}", Address.ToString(), User != null ? User.Username : "UDP NULL"); } }

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

        public abstract void ReceivedData(byte[] data);

        internal UDPNetState(IPEndPoint ep, int readBufferSize)
            : base(ep)
        {
            Reader = new TReader();
            Writer = new TWriter();

            Writer.SetEndpoint(ep);

            Reader.OnDisconnect += Dispose;
            Reader.OnBufferOverflow += Reader_OnBufferOverflow;

            OnConnected();
        }

        protected abstract void Reader_OnBufferOverflow(object sender, BufferOverflowArgs e);

        protected virtual void OnConnected() { }

        public sealed override void Write(byte[] bytes)
        {
            Writer.Write(bytes);
        }

        public override void Dispose()
        {
            base.Dispose();

            Reader.Dispose();
            Writer.Dispose();
        }
    }

}
