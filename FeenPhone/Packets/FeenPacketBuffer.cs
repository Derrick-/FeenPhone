using Alienseed.BaseNetworkServer.PacketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone.Packets
{
    class FeenPacketBuffer : PacketBuffer<FeenPacketBuffer>
    {
        protected override int MaxLength { get { return ushort.MaxValue; } }

        [Obsolete ("FeenPacketBuffer should be Acquired, not instanciated.")]
        public FeenPacketBuffer() : base(null) { }
        public new static FeenPacketBuffer Acquire(int? initialSize = null)
        {
            return PacketBuffer<FeenPacketBuffer>.Acquire(initialSize);
        }

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
                WriteLength(0);
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

        internal void Write(Audio.Codecs.CodecID Codec)
        {
            Write((byte)Codec);
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
