using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone.Packets
{
    class FeenPacketBuffer : Alienseed.BaseNetworkServer.PacketServer.PacketBuffer
    {
        protected override int MaxLength { get { return ushort.MaxValue; } }

        public FeenPacketBuffer(int? initialSize = null) : base(initialSize) { }

        public FeenPacketBuffer(PacketID packetID, byte[] payload)
        {
            Write(packetID, payload);
        }

        internal void Write(PacketID packetID, byte[] payload)
        {
            if (payload.Length > ushort.MaxValue)
                throw new ArgumentException("Packet overflow", "payload");

            Write(packetID);
            if (payload == null)
                Write((ushort)0);
            else
            {
                WriteLength(payload);
                Write(payload);
            }
        }

        internal void Write(PacketID packetID)
        {
            Write((byte)packetID);
        }

        internal void WriteLength(byte[] payload)
        {
            WriteLength(payload.Length);
        }

        internal void WriteLength(int payloadLength)
        {
            if (payloadLength > MaxLength)
                throw new ArgumentException(string.Format("Packet length({0}) exceeds MaxLength({0})", payloadLength, MaxLength), "payloadLength");

            Write((ushort)payloadLength);
        }
    }
}
