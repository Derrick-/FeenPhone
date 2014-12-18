using Alienseed.BaseNetworkServer.Accounting;
using Alienseed.BaseNetworkServer.PacketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Alienseed.BaseNetworkServer
{
    public abstract class BaseTCPServer<TReader, TWriter, Tnetstate> : BaseStateServer<Tnetstate>
        where Tnetstate : TCPNetState<TReader, TWriter>
        where TReader : BaseStreamReader, new()
        where TWriter : BaseStreamWriter, new()
    {
        public BaseTCPServer(int port, IPAddress address = null, bool noDelay=false)
            : base(port, address) 
        {
            NoDelay = noDelay;
        }

        public bool NoDelay { get; private set; }

        #region INetworkServer Members

        public override bool Start()
        {
            try
            {
                Listener.Start();
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return Running = Listen();
        }

        public override void Stop()
        {
            Listener.Stop();
            Running = false;
            PurgeAllClients();
        }

        #endregion

        #region Listener

        private TcpListener _Listener;
        private TcpListener Listener { get { return _Listener ?? (_Listener = CreateListener()); } }

        private TcpListener CreateListener()
        {
            var listener = new TcpListener(Address, Port);
            return listener;
        }
        private void RecycleListener()
        {
            if (_Listener != null) _Listener.Stop();
            _Listener = null;
        }

        bool Listen()
        {
            try
            {
                Listener.BeginAcceptTcpClient(AcceptClient, Listener);
            }
            catch (SocketException ex)
            {
                var error = ex.SocketErrorCode;
                Console.WriteLine(string.Format("Listener Error: ({1}) {0}", ex, error));
                RecycleListener();
                return false;
            }
            return true;
        }

        protected abstract Tnetstate CreateNetstate(System.Net.Sockets.NetworkStream stream, EndPoint ep);

        private void AcceptClient(IAsyncResult ar)
        {
            if (ar.AsyncState != null && ar.AsyncState == Listener)
            {
                try
                {
                    TcpClient client = Listener.EndAcceptTcpClient(ar);
                    client.NoDelay = NoDelay;
                    NetworkStream stream = client.GetStream();
                    var ns = CreateNetstate(stream, client.Client.RemoteEndPoint);
                    base.AcceptClient(ns);

                }
                catch (ObjectDisposedException) { }

                if (Running && !Listen())
                    InvokeOnListenerCrash();

            }
        }

        #endregion
    }
}
