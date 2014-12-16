using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Alienseed.BaseNetworkServer
{
    public abstract class BaseServer : INetworkServer
    {
        public static IEnumerable<INetState> Clients { get { return NetState.Clients; } }
        public static IEnumerable<IUser> Users { get { return NetState.Clients.Select(m => m.User); } }
        
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

        public event OnServerCrashHandler OnCrash;

        protected void InvokeOnListenerCrash()
        {
            if (OnCrash != null)
                OnCrash(this);
        }
    }

    public abstract class BaseStateServer<TReader, TWriter, Tnetstate> : BaseServer
        where Tnetstate : NetState
        where TReader : BaseStreamReader, new()
        where TWriter : BaseStreamWriter, new()
    {
        public delegate void OnClientConnectionHandler(Tnetstate client);
        public delegate void OnClientLoginLogoutHandler(IUserClient client);

        public event OnClientConnectionHandler OnClientConnected;
        public event OnClientConnectionHandler OnClientDisconnected;
        public event OnClientLoginLogoutHandler OnClientLogin;
        public event OnClientLoginLogoutHandler OnClientLogout;

        public BaseStateServer(int port, IPAddress address = null)
            : base(port, address)
        {
            this.OnClientConnected += ClientConnected;
            this.OnClientDisconnected += ClientDisconnected;
            this.OnClientLogin += ClientLogin;
            this.OnClientLogout += ClientLogout;
        }

        protected abstract void ClientConnected(Tnetstate client);
        protected abstract void ClientDisconnected(Tnetstate client);
        protected abstract void ClientLogin(IUserClient client);
        protected abstract void ClientLogout(IUserClient client);

        protected abstract void PurgeAllClients();

        protected abstract Tnetstate CreateNetstate(NetworkStream stream, EndPoint ep);

        protected internal void AcceptClient(Tnetstate ns)
        {
            ns.OnDisposed += NetState_OnDisposed;
            ns.OnLogin += NetState_OnLogin;
            ns.OnLogout += NetState_OnLogout;
            InvokeOnClientConnected(ns);
        }
        
        private void InvokeOnClientConnected(Tnetstate ns)
        {
            if (OnClientConnected != null)
                OnClientConnected(ns);
        }

        private void InvokeOnClientDisconnected(Tnetstate ns)
        {
            if (OnClientDisconnected != null)
                OnClientDisconnected(ns);
        }

        private void InvokeOnClientLogin(IUserClient userClient)
        {
            if (OnClientLogin != null)
                OnClientLogin(userClient);
        }

        private void InvokeOnClientLogout(IUserClient userClient)
        {
            if (OnClientLogout != null)
                OnClientLogout(userClient);
        }


        private void NetState_OnDisposed(object sender, NetState.OnDisposedEventArgs e)
        {
            InvokeOnClientDisconnected(e.State as Tnetstate);
        }

        private void NetState_OnLogin(object sender, NetState.OnLoginLogoutEventArgs e)
        {
            InvokeOnClientLogin(e.Client);
        }

        private void NetState_OnLogout(object sender, NetState.OnLoginLogoutEventArgs e)
        {
            InvokeOnClientLogout(e.Client);
        }
    }

    public abstract class BaseTCPServer<TReader, TWriter, Tnetstate> : BaseStateServer<TReader, TWriter, Tnetstate> 
        where Tnetstate : NetState
        where TReader : BaseStreamReader, new()
        where TWriter : BaseStreamWriter, new()
    {
        public BaseTCPServer(int port, IPAddress address = null)
            : base(port, address)
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
