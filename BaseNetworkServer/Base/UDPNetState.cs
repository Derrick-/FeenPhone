using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Alienseed.BaseNetworkServer
{
    public abstract class UDPNetState<TReader, TWriter> : NetState, IDisposable
        where TReader : BaseUDPReader, new()
        where TWriter : BaseUDPWriter, new()
    {
        public readonly TReader Reader;
        public readonly TWriter Writer;

        protected override string ClientIdentifier { get { return string.Format("UDP {0} {1}", Address.ToString(), User != null ? User.Username : "UDP NULL"); } }

        public DateTime LastActivity { get; set; }

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

        internal System.Net.Sockets.UdpClient Client = null;

        internal UDPNetState(IPEndPoint ep, int readBufferSize)
            : base(ep)
        {
            Reader = new TReader();
            Writer = new TWriter();

            LastActivity = DateTime.UtcNow;

            Client = new UdpClient();
            Client.Connect(ep);
            Writer.SetClient(Client);

            Client.BeginReceive(new AsyncCallback(OnReceived), null);

            Reader.OnDisconnect += Dispose;
            Reader.OnBufferOverflow += Reader_OnBufferOverflow;

            OnConnected();
        }

        private void OnReceived(IAsyncResult ar)
        {
            if (Client != null)
            {
                IPEndPoint endpoint = new IPEndPoint((EndPoint as IPEndPoint).Address, 0);

                byte[] data;
                try
                {
                    data = Client.EndReceive(ar, ref endpoint);
                }
                catch (SocketException)
                {
                    Dispose();
                    return;
                }
                catch (ObjectDisposedException)
                {
                    Dispose();
                    return;
                }
                LastActivity = DateTime.UtcNow;
                Client.BeginReceive(new AsyncCallback(OnReceived), null);
                Reader.ReceivedData(data);
            }
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
            if (Client != null)
                Client.Close();
            Client = null;
            Reader.Dispose();
            Writer.Dispose();
        }
    }

}
