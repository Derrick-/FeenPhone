using Alienseed.BaseNetworkServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FeenPhone.Server
{
    class ServerHost : IDisposable
    {

        List<BaseServer> Servers = new List<BaseServer>();

        static Client.LocalClient _LocalClient = null;
        internal static Client.LocalClient LocalClient
        {
            get { return _LocalClient; }
            set
            {
                if (value != _LocalClient && _LocalClient != null)
                {
                    _LocalClient.Dispose();
                }
                _LocalClient = value;
            }
        }

        public ServerHost(int TcpServerPort = TcpPacketServer.TcpPacketServer.DefaultTcpServerPort)
        {
            InitServers();
            StartServers();
        }

        void InitServers(int TcpServerPort = TcpPacketServer.TcpPacketServer.DefaultTcpServerPort)
        {
            StopServers();
            Servers.Clear();

            Servers.Add(new Telnet.TelnetServer());
            Servers.Add(new TcpPacketServer.TcpPacketServer(TcpServerPort));
        }

        void StartServers()
        {
            var failed = new List<BaseServer>();

            foreach (var server in Servers.Where(m => !m.Running))
            {
                Console.WriteLine("STARTING {0} on port {1}", server, server.Port);
                if (server.Start())
                {
                    Console.WriteLine("Started server: {0} on port {1}", server, server.Port);
                    server.OnCrash += server_OnCrash;
                }
                else
                {
                    Console.WriteLine("FAILED server: {0} on port {1}", server, server.Port);
                    failed.Add(server);
                }
            }

            foreach (var fail in failed)
                Servers.Remove(fail);

        }

        private void StopServers()
        {
            foreach (var server in Servers.Where(m => m.Running))
            {
                Console.WriteLine("Stopping server: {0}", server);
                server.Stop();
            }
        }

        void server_OnCrash(INetworkServer server)
        {
            Console.WriteLine("Server Crash: {0} on Port {1}", server, server.Port);

            server.Stop();
            if (server is BaseServer)
                Servers.Remove((BaseServer)server);
        }

        #region IDisposable Members

        public void Dispose()
        {
            StopServers();
            Servers.Clear();
        }

        #endregion
    }
}


