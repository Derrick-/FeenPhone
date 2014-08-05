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

        internal static void OnChat(Telnet.TelNetState client, string text)
        {
            foreach (var feen in AllFeens)
                if (feen != client)
                    feen.OnChat(client.User, text);
        }
    }
}
