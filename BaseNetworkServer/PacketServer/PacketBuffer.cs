using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alienseed.BaseNetworkServer.PacketServer
{
    public class PacketBuffer : IDisposable
    {
        private Queue<byte> Bytes;

        protected virtual int MaxLength { get { return int.MaxValue; } }

        protected int CurrentLength
        {
            get { return Bytes.Count(); }
        }

        public PacketBuffer(int? initialSize = null)
        {
            if (initialSize.HasValue)
                Bytes = new Queue<byte>(initialSize.Value);
            else
                Bytes = new Queue<byte>();
        }

        byte[] data = null;
        private void Invalidate() { data = null; }
        public byte[] GetData()
        {
            return data ?? (data = Bytes.ToArray());
        }

        public void Write(byte value)
        {
            if (CurrentLength + 1 > MaxLength)
                throw new ArgumentException("Packet overflow", "value");
            Bytes.Enqueue(value);
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

        public void Write(byte[] data, int? length = null)
        {
            if (length == null)
                length = data.Length;
            else
                length = Math.Min(length.Value, data.Length);

            if (CurrentLength + length > MaxLength)
                throw new ArgumentException("Packet overflow", "data");

            IEnumerable<byte> bytes = data;
            if (length < data.Length)
                bytes = bytes.Take(length.Value);

            if (length > 100)
            {
                Bytes = new Queue<byte>(Bytes.Concat(bytes));
                Invalidate();
            }
            else
            {
                foreach (byte value in bytes)
                    Write(value);
            }
        }

        public void Dispose()
        {
            Bytes = null;
            data = null;
        }
    }
}
