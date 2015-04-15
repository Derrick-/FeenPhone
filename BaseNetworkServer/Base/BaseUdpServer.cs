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
            try
            {
                Host.BeginReceive(new AsyncCallback(Host_RecieveCallback), state);
            }
            catch (SocketException)
            {
                if (Running)
                    Listen();
            }
            catch (NullReferenceException) { }
        }

        private void Host_RecieveCallback(IAsyncResult ar)
        {
            if (Host == ((UdpState)(ar.AsyncState)).host)
            {
                IPEndPoint endpoint = ((UdpState)(ar.AsyncState)).endpoint;

                Tnetstate ns = null;

                byte[] data = null;
                try
                {
                    data = ((UdpState)(ar.AsyncState)).host.EndReceive(ar, ref endpoint);
                    ns = NetstateFactory(endpoint);
                    int clientPort = ((IPEndPoint)(ns.Client.Client.LocalEndPoint)).Port;
                    Host.Send(new byte[] { (byte)(clientPort >> 8), (byte)clientPort }, 2, endpoint);
                    base.AcceptClient(ns);
                }
                catch (SocketException)
                {
                }
                catch (ObjectDisposedException)
                {
                    Stop();
                    return;
                }

                Listen();
            }
        }

        protected abstract Tnetstate NetstateFactory(EndPoint ep);

        public override void Stop()
        {
            Running = false;
            if (Host != null)
            {
                Host.Close();
                Host = null;
                PurgeAllClients();
            }
        }

    }
}
