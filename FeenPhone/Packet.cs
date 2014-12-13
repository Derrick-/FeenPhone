using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone
{
    static class Packet
    {

        internal static void WriteChat(Alienseed.BaseNetworkServer.PacketServer.NetworkPacketWriter Writer, string text)
        {
            byte[] data = Encoding.ASCII.GetBytes(text);
            Writer.Write(new byte[] { 1, (byte)(text.Length >> 1), (byte)text.Length });
            Writer.Write(data);
        }
    }
}
