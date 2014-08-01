using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Alienseed.BaseNetworkServer.Network
{
    public abstract class NetworkServer
    {
        public static IEnumerable<INetState> Clients { get { return NetState.AllClients; } }

        public static IEnumerable<IUser> AllUsers { get { return NetworkServer.Clients.Select(m => m.User); } }
    }

    public abstract class BaseServer : NetworkServer, INetworkServer
    {
        public BaseServer(int port, IPAddress address = null)
        {
            Address = address ?? IPAddress.Any;
            Port = port;
        }

        public IPAddress Address { get; private set; }
        public int Port { get; private set; }

        public abstract bool Start();

        public abstract void Stop();

        public abstract bool Running { get; protected set; }

        public event OnListenerCrashHandler OnListenerCrash;

        protected void InvokeOnListenerCrash()
        {
            if (OnListenerCrash != null)
                OnListenerCrash(this);
        }

    }

    public abstract class BaseTCPServer<TReader, TWriter> : BaseServer
        where TReader : BaseStreamReader, new()
        where TWriter : BaseStreamWriter, new()
    {
        public delegate void OnClientConnectedHandler(TCPNetState<TReader, TWriter> client);
        public event OnClientConnectedHandler OnClientConnected;

        public BaseTCPServer(int port, IPAddress address = null) : base(port, address)
        {
        }

        #region INetworkServer Members

        public override bool Running { get; protected set; }

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

        protected abstract void PurgeAllClients();

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

        private void AcceptClient(IAsyncResult ar)
        {
            if (ar.AsyncState != null && ar.AsyncState == Listener)
            {
                try
                {
                    TcpClient client = Listener.EndAcceptTcpClient(ar);
                    NetworkStream stream = client.GetStream();

                    var ns = CreateNetstate(stream, client.Client.RemoteEndPoint);
                    InvokeOnClientConnected(ns);

                }
                catch (ObjectDisposedException) { }

                if (Running && !Listen())
                    InvokeOnListenerCrash();

            }
        }

        protected abstract TCPNetState<TReader, TWriter> CreateNetstate(NetworkStream stream, EndPoint ep);

        #endregion

        private void InvokeOnClientConnected(TCPNetState<TReader, TWriter> ns)
        {
            if (OnClientConnected != null)
                OnClientConnected(ns);
        }

    }
}
