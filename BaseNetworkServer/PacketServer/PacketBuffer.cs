using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alienseed.BaseNetworkServer.PacketServer
{
    public class PacketBuffer : IDisposable
    {
        private int BytesLength = 0;
        private byte[] Bytes;

        protected virtual int DefaultSize { get { return 1024; } }
        protected virtual int MaxLength { get { return int.MaxValue; } }

        public PacketBuffer(int? initialSize = null)
        {
            if (initialSize.HasValue)
                Bytes = new byte[initialSize.Value];
            else
                Bytes = new byte[DefaultSize];
        }

        byte[] data = null;
        private void Invalidate() { data = null; }
        public byte[] GetData()
        {
            return data ?? (data = Bytes.Take(BytesLength).ToArray());
        }

        public void Write(byte value)
        {
            EnsureCapacity(BytesLength + 1);
            Bytes[BytesLength++] = value;
            Invalidate();
        }

        private void EnsureCapacity(int requiredLength)
        {
            if (requiredLength > MaxLength)
                throw new ArgumentException("Exceeded max buffer length", "requiredLength");
            if (Bytes.Length < requiredLength)
            {
                int newlength = Bytes.Length < MaxLength / 2 ? requiredLength * 2 : MaxLength;
                byte[] newBytes = new byte[newlength];
                Buffer.BlockCopy(Bytes, 0, newBytes, 0, BytesLength);
                Bytes = newBytes;
            }
        }

        public void Write(bool value)
        {
            Write(value ? (byte)0xFF : (byte)0);
        }

        public void Write(ushort value)
        {
            EnsureCapacity(BytesLength + 2);
            Bytes[BytesLength++] = (byte)(value >> 8);
            Bytes[BytesLength++] = (byte)value;
        }

        public void Write(byte[] data, int? optLength = null)
        {
            int length;
            if (optLength == null)
                length = data.Length;
            else
                length = Math.Min(optLength.Value, data.Length);

            int newLength = BytesLength + length;

            EnsureCapacity(newLength);
            Buffer.BlockCopy(data, 0, Bytes, BytesLength, length);
            BytesLength = newLength;
        }

        public void Dispose()
        {
            Bytes = null;
            data = null;
        }
    }
}
