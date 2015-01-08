using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alienseed.BaseNetworkServer.PacketServer
{
    public interface IPacketBuffer : IDisposable
    {
        int BytesLength { get; }
        byte[] GetData();
        void Write(bool value);
        void Write(byte value);
        void Write(byte[] data, int? optLength = null);
        void Write(ushort value);
    }

    public abstract class PacketBuffer<T> : IPacketBuffer where T : PacketBuffer<T>, new()
    {
        public static List<T> Buffers = new List<T>();

        private static object bufferLock = new object();
        public static T Acquire(int? initialSize = null)
        {
            int minSize = initialSize.HasValue ? initialSize.Value : 0;
            T buffer = default(T);
            lock (bufferLock)
            {
                buffer = Buffers.Where(m => m.Bytes.Length >= minSize).FirstOrDefault();
                if (buffer == null)
                    buffer = new T() { WasAcquired = true };
                buffer.isDisposed = false;
                Buffers.Remove(buffer);
            }
            return buffer;
        }

        private static void ReturnBufffer(T buffer)
        {
            lock (bufferLock)
                Buffers.Add(buffer);
        }

        protected bool WasAcquired { get; set; }

        public int BytesLength { get; private set; }
        private byte[] Bytes;

        protected virtual int DefaultSize { get { return 1024; } }
        protected virtual int MaxLength { get { return int.MaxValue; } }

        protected PacketBuffer(int? initialSize = null)
        {
            if (initialSize.HasValue)
                Bytes = new byte[initialSize.Value];
            else
                Bytes = new byte[DefaultSize];
            WasAcquired = false;
        }

        public byte[] GetData()
        {
            return Bytes;
        }

        public void Write(byte value)
        {
            EnsureCapacity(BytesLength + 1);
            Bytes[BytesLength++] = value;
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

        protected bool isDisposed = false;
        public void Dispose()
        {
            if (!isDisposed && Bytes != null && WasAcquired)
            {
                BytesLength = 0;
                ReturnBufffer(this as T);
            }
            else
                Bytes = null;
            isDisposed = true;
        }
    }
}
