using Alienseed.BaseNetworkServer.Accounting;
using Alienseed.BaseNetworkServer.PacketServer;
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

    public abstract class BaseStateServer<Tnetstate> : BaseServer
        where Tnetstate : NetState
    {
        public delegate void OnClientConnectionHandler(Tnetstate client);
        public delegate void OnClientLoginLogoutHandler(IUserClient client);

        public event OnClientConnectionHandler OnClientConnected;
        public event OnClientConnectionHandler OnClientDisconnected;
        public event OnClientLoginLogoutHandler OnClientLogin;
        public event OnClientLoginLogoutHandler OnClientLogout;

        public override bool Running { get; protected set; }

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
}
