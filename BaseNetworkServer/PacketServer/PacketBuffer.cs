using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alienseed.BaseNetworkServer.PacketServer
{
    public class PacketBuffer : IDisposable
    {
        Queue<byte> bytes;

        protected virtual int MaxLength { get { return int.MaxValue; } }

        protected int CurrentLength
        {
            get { return bytes.Count(); }
        }

        public PacketBuffer(int? initialSize = null)
        {
            if (initialSize.HasValue)
                bytes = new Queue<byte>(initialSize.Value);
            else
                bytes = new Queue<byte>();
        }

        byte[] data = null;
        private void Invalidate() { data = null; }
        public byte[] GetData()
        {
            return data ?? (data = bytes.ToArray());
        }

        public void Write(byte value)
        {
            if (CurrentLength + 1 > MaxLength)
                throw new ArgumentException("Packet overflow", "value");
            bytes.Enqueue(value);
            Invalidate();
        }

        public void Write(bool value)
        {
            Write(value ? (byte)0xFF : (byte)0);
        }

        public void Write(ushort value)
        {
            Write((byte)(value >> 8));
            Write((byte)value);
        }

        public void Dispose()
        {
            bytes = null;
            data = null;
        }

        public void Write(byte[] data)
        {
            if (CurrentLength + data.Length > MaxLength)
                throw new ArgumentException("Packet overflow", "data");
            foreach (byte value in data)
                Write(value);
        }
    }
}
