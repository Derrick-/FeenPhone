using Alienseed.BaseNetworkServer.Accounting;
using Alienseed.BaseNetworkServer.PacketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Alienseed.BaseNetworkServer
{
    public abstract class BaseUDPServer<TReader, TWriter, Tnetstate> : BaseStateServer<Tnetstate>
        where Tnetstate : UDPNetState<TReader, TWriter>
        where TReader : UDPPacketReader, new()
        where TWriter : UDPPacketWriter, new()
    {
        public BaseUDPServer(int port)
            : base(port, IPAddress.Any) { }

        private IPEndPoint sender;
        private UdpClient Host;

        class Connection
        {
            public DateTime LastActivity { get; set; }

            public readonly Tnetstate Netstate;

            public Connection(Tnetstate netstate)
            {
                Netstate = netstate;
            }
        }

        Dictionary<IPEndPoint, Connection> Connections = new Dictionary<IPEndPoint, Connection>();

        class UdpState
        {
            public UdpClient host { get; set; }
            public IPEndPoint endpoint { get; set; }
        }

        public override bool Start()
        {
            Host = new UdpClient(Port);

            Listen();

            return Running = true;
        }

        private void Listen()
        {
            sender = new IPEndPoint(Address, 0);
            UdpState state = new UdpState();
            state.endpoint = sender;
            state.host = Host;
            Host.BeginReceive(new AsyncCallback(Host_RecieveCallback), state);
        }

        private void Host_RecieveCallback(IAsyncResult ar)
        {
            if (Host == ((UdpState)(ar.AsyncState)).host)
            {
                IPEndPoint endpoint = ((UdpState)(ar.AsyncState)).endpoint;

                var data = Host.EndReceive(ar, ref endpoint);

                Connection connection;
                if (!Connections.ContainsKey(endpoint))
                {
                    Tnetstate ns = NetstateFactory(endpoint);
                    connection = new Connection(ns);
                    base.AcceptClient(ns);
                }
                else
                    connection = Connections[endpoint];

                connection.Netstate.ReceivedData(data);

                connection.LastActivity = DateTime.Now;

                Listen();
            }
        }

        protected abstract Tnetstate NetstateFactory(EndPoint ep);

        public override void Stop()
        {
            if (Host != null)
            {
                Host.Close();
                Host = null;
            }
            Running = false;
        }

    }
}
