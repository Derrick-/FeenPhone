using Alienseed.BaseNetworkServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FeenPhone.Server
{
    class ServerHost : IDisposable
    {
        public const int DefaultServerPort = 5150;

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

        public const int PingTimerIntervalMs = 250;
        Timer PingTimer;
        public ServerHost()
        {
            PingTimer = new Timer(PingTimer_Elapsed, this, PingTimerIntervalMs, PingTimerIntervalMs);
        }

        private int lastClientIndex = 0;
        private void PingTimer_Elapsed(object state)
        {
            var clients = BaseServer.Clients.Where(m => m is IFeenPhonePacketNetState).Cast<IFeenPhonePacketNetState>().ToArray();
            int count = clients.Count();
            if (count > 0)
            {
                IFeenPhonePacketNetState client;
                if (lastClientIndex >= count)
                {
                    lastClientIndex = 0;
                    client = clients.First();
                }
                else
                    client = clients.Skip(lastClientIndex++).First();
                Packet.WritePingReq(client.Writer);
            }
        }

        private int _TCPServerPort = DefaultServerPort;
        private int _UDPServerPort = DefaultServerPort;
        private int _TelnetServerPort = 23;

        public int TCPServerPort
        {
            get { return _TCPServerPort; }
            set
            {
                bool shouldRestart = _TCPServerPort != value && Servers.Any(m => m is PacketServer.TcpPacketServer);
                _TCPServerPort = value;
                if (shouldRestart)
                {
                    EnableTCP(false);
                    EnableTCP(true);
                }
            }
        }

        public int UDPServerPort
        {
            get { return _UDPServerPort; }
            set
            {
                bool shouldRestart = _TCPServerPort != value && Servers.Any(m => m is PacketServer.UdpPacketServer);
                _UDPServerPort = value;
                if (shouldRestart)
                {
                    EnableUDP(false);
                    EnableUDP(true);
                }
            }
        }

        public int TelnetServerPort
        {
            get { return _TelnetServerPort; }
            set
            {
                bool shouldRestart = _TCPServerPort != value && Servers.Any(m => m is Telnet.TelnetServer);
                _TelnetServerPort = value;
                if (shouldRestart)
                {
                    EnableTelnet(false);
                    EnableTelnet(true);
                }
            }
        }

        public void InitServers(bool TCP = true, bool UDP = true, bool Telnet = true)
        {
            StopAllServers();

            if (TCP)
                EnableTCP(true);

            if (UDP)
                EnableUDP(true);

            if (Telnet)
                EnableTelnet(true);
        }

        bool StartServer(BaseServer server)
        {
            bool failed = false;

            Console.WriteLine("STARTING {0} on port {1}", server, server.Port);
            if (server.Start())
            {
                Console.WriteLine("Started server: {0} on port {1}", server, server.Port);
                server.OnCrash += server_OnCrash;
            }
            else
            {
                Console.WriteLine("FAILED server: {0} on port {1}", server, server.Port);
                failed = true;
            }

            return failed;

        }

        private void StopAllServers()
        {
            foreach (var server in Servers.Where(m => m.Running))
            {
                Console.WriteLine("Stopping server: {0}", server);
                server.Stop();
            }
            Servers.Clear();
        }

        internal void EnableTCP(bool enable)
        {
            var existing = Servers.SingleOrDefault(m => m is PacketServer.TcpPacketServer);
            if (enable)
            {
                if (existing == null)
                    Servers.Add(existing = new PacketServer.TcpPacketServer(TCPServerPort, noDelay: true));
                if (StartServer(existing))
                    Servers.Remove(existing);
            }
            else if (existing != null)
            {
                existing.Stop();
                Servers.Remove(existing);
                Console.WriteLine("Stopped {0}.", existing);
            }
        }

        internal void EnableUDP(bool enable)
        {
            var existing = Servers.SingleOrDefault(m => m is PacketServer.UdpPacketServer);
            if (enable)
            {
                if (existing == null)
                    Servers.Add(existing = new PacketServer.UdpPacketServer(UDPServerPort));
                if (StartServer(existing))
                    Servers.Remove(existing);
            }
            else if (existing != null)
            {
                existing.Stop();
                Servers.Remove(existing);
                Console.WriteLine("Stopped {0}.", existing);
            }
        }

        internal void EnableTelnet(bool enable)
        {
            var existing = Servers.SingleOrDefault(m => m is Telnet.TelnetServer);
            if (enable)
            {
                if (existing == null)
                    Servers.Add(existing = new Telnet.TelnetServer(TelnetServerPort));
                if (StartServer(existing))
                    Servers.Remove(existing);
            }
            else if (existing != null)
            {
                existing.Stop();
                Servers.Remove(existing);
                Console.WriteLine("Stopped {0}.", existing);
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
            StopAllServers();
        }

        #endregion
    }
}


