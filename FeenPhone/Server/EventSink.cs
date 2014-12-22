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
        static IEnumerable<IFeenPhoneClientNotifier> AllFeens
        {
            get { return BaseServer.Clients.Where(m => m is IFeenPhoneNetstate).Cast<IFeenPhoneNetstate>().Select(m => m.Notifier).ToList().Where(m => m != null); }
        }

        internal static void OnChat(IFeenPhoneNetstate state, string text)
        {
            foreach (var feen in AllFeens)
                if (feen != state.Notifier)
                    if (state.User != null)
                        feen.OnChat(state, text);
            if (ServerHost.LocalClient != null)
                ServerHost.LocalClient.Notifier.OnChat(state, text);
        }

        internal static void OnAudio(IFeenPhoneNetstate state, Guid userID, Audio.Codecs.CodecID Codec, byte[] data, int dataLen)
        {
            foreach (var feen in AllFeens)
                if (feen != state.Notifier)
                    feen.OnAudio(userID, Codec, data, dataLen);
            if (ServerHost.LocalClient != null && ServerHost.LocalClient != state)
                ServerHost.LocalClient.Notifier.OnAudio(userID, Codec, data, dataLen);
        }

        internal static void OnConnect(IFeenPhoneNetstate state)
        {
            foreach (var feen in AllFeens)
                if (feen != state.Notifier)
                    feen.OnUserConnected(state);
            if (ServerHost.LocalClient != null)
                ServerHost.LocalClient.Notifier.OnUserConnected(state);
        }

        internal static void OnDisconnect(IFeenPhoneNetstate state)
        {
            foreach (var feen in AllFeens.ToList())
                if (feen != state.Notifier)
                    feen.OnUserDisconnected(state);
            if (ServerHost.LocalClient != null)
                ServerHost.LocalClient.Notifier.OnUserDisconnected(state);
        }

        internal static void OnLogin(IFeenPhoneNetstate state)
        {
            foreach (var feen in AllFeens)
                if (feen != state.Notifier)
                    feen.OnUserLogin(state.User);
            if (ServerHost.LocalClient != null)
                ServerHost.LocalClient.Notifier.OnUserLogin(state.User);
        }

        internal static void OnLogout(IFeenPhoneNetstate state)
        {
            foreach (var feen in AllFeens)
                if (feen != state.Notifier)
                    feen.OnUserLogout(state.User);
            if (ServerHost.LocalClient != null)
                ServerHost.LocalClient.Notifier.OnUserLogout(state.User);
        }
    }
}
