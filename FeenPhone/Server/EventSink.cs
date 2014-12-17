using Alienseed.BaseNetworkServer.Accounting;
using Alienseed.BaseNetworkServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone.Server
{
    static class EventSink
    {
        static IEnumerable<IFeenPhoneEvents> AllFeens
        {
            get { return BaseServer.Clients.Where(m => m is IFeenPhoneEvents).Cast<IFeenPhoneEvents>().ToList(); }
        }

        internal static void OnChat(INetState client, string text)
        {
            foreach (var feen in AllFeens)
                if (feen != client)
                    if (client.User != null)
                        feen.OnChat(client, text);
            if (ServerHost.LocalClient != null)
                ServerHost.LocalClient.OnChat(client, text);
        }

        internal static void OnAudio(INetState client, Audio.Codecs.CodecID Codec, byte[] data, int dataLen)
        {
            foreach (var feen in AllFeens)
                if (feen != client)
                    feen.OnAudio(Codec, data, dataLen);
            if (ServerHost.LocalClient != null && ServerHost.LocalClient != client)
                ServerHost.LocalClient.OnAudio(Codec, data, dataLen);
        }

        internal static void OnConnect(INetState client)
        {
            foreach (var feen in AllFeens)
                if (feen != client)
                    feen.OnUserConnected(client);
            if (ServerHost.LocalClient != null)
                ServerHost.LocalClient.OnUserConnected(client);
        }

        internal static void OnDisconnect(INetState client)
        {
            foreach (var feen in AllFeens.ToList())
                if (feen != client)
                    feen.OnUserDisconnected(client);
            if (ServerHost.LocalClient != null)
                ServerHost.LocalClient.OnUserDisconnected(client);
        }

        internal static void OnLogin(IUserClient client)
        {
            foreach (var feen in AllFeens)
                if (feen != client)
                    feen.OnUserLogin(client);
            if (ServerHost.LocalClient != null)
                ServerHost.LocalClient.OnUserLogin(client);
        }

        internal static void OnLogout(IUserClient client)
        {
            foreach (var feen in AllFeens)
                if (feen != client)
                    feen.OnUserLogout(client);
            if (ServerHost.LocalClient != null)
                ServerHost.LocalClient.OnUserLogout(client);
        }
    }
}
