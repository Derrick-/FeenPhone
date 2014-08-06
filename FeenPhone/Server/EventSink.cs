using Alienseed.BaseNetworkServer.Accounting;
using Alienseed.BaseNetworkServer.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone.Server
{
    static class EventSink
    {
        static IEnumerable<IFeenPhoneNetstate> AllFeens
        {
            get { return NetworkServer.Clients.Where(m => m is IFeenPhoneNetstate).Cast<IFeenPhoneNetstate>(); }
        }

        internal static void OnChat(NetState client, string text)
        {
            foreach (var feen in AllFeens)
                if (feen != client)
                    if(client.User!=null)
                        feen.OnChat(client, text);
        }

        internal static void OnConnect(NetState client)
        {
            foreach (var feen in AllFeens)
                if (feen != client)
                    feen.OnUserConnected(client);
        }

        internal static void OnDisconnect(NetState client)
        {
            foreach (var feen in AllFeens)
                if (feen != client)
                    feen.OnUserDisconnected(client);
        }


        internal static void OnLogin(IUserClient client)
        {
            foreach (var feen in AllFeens)
                if (feen != client)
                    feen.OnUserLogin(client);
        }

        internal static void OnLogout(IUserClient client)
        {
            foreach (var feen in AllFeens)
                if (feen != client)
                    feen.OnUserLogout(client);
        }
    }
}
