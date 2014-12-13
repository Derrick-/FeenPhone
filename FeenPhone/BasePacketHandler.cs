using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace FeenPhone
{

    abstract class BasePacketHandler
    {

        Dictionary<int, Handler> Handlers;

        public BasePacketHandler()
        {
            Handlers = new Dictionary<int, Handler>()
                {
                    {1, OnChat}
                };
        }

        protected delegate void Handler(IEnumerable<byte> payload);

        public bool ValidPacketID(int id)
        {
            return Handlers.ContainsKey(id);
        }

        public void Handle(Queue<byte> data)
        {
            Handler handler;
            ushort len;
            int consumed = Parse(data.ToArray(), out handler, out len);

            if (handler != null)
                handler(data.Skip(3).Take(len));

            for (int i = 0; i < consumed && data.Any(); i++)
            {
                data.Dequeue();
            }
        }

        protected int Parse(byte[] data, out Handler handler, out ushort len)
        {
            handler = null;
            len = 0;
            int i = 0;
            while (i < data.Length - 2 && !ValidPacketID(data[i]))
            {
                i++;
            }

            if (!ValidPacketID(data[i]) && i <= data.Length - 2)
            {
                return i;
            }

            int packetid = data[i];
            len = (ushort)(data[i + 1] << 1 | data[i + 2]);
            if (data.Length < len + 3)
                return i;
            IEnumerable<byte> payload = data.Skip(3).Take(len);

            handler = Handlers[packetid];
            return len + 3;
        }

        protected abstract void OnChat(IEnumerable<byte> payload);

    }
}
