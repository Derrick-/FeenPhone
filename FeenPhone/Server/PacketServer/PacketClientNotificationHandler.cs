using Alienseed.BaseNetworkServer;
using Alienseed.BaseNetworkServer.PacketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeenPhone.Server.PacketServer
{
    class PacketClientNotificationHandler : IFeenPhoneClientNotifier
    {
        private readonly IFeenPhoneNetstate State;
        private readonly IPacketWriter Writer;

        public PacketClientNotificationHandler(IFeenPhonePacketNetState state)
        {
            this.State = state;
            this.Writer = state.Writer;
        }

        public void LoginSuccess()
        {
            Packet.WriteLoginStatus(Writer, true);

            var users = BaseServer.Users.Where(m => m != null);
            if (ServerHost.LocalClient != null)
            {
                users = users.Concat(new Alienseed.BaseNetworkServer.Accounting.IUser[] { ServerHost.LocalClient.LocalUser });
            }
            Packet.WriteUserList(Writer, users.Where(m => m.ID != State.User.ID));
        }

        public void LoginFailed()
        {
            Packet.WriteLoginStatus(Writer, false);
        }

        public void OnUserConnected(Alienseed.BaseNetworkServer.INetState state)
        {
            if (state.User != null)
                Packet.WriteUserLogin(Writer, state.User);
        }

        public void OnUserDisconnected(Alienseed.BaseNetworkServer.INetState state)
        {
            if (state.User != null)
                Packet.WriteUserLogout(Writer, state.User);
        }

        public void OnUserLogin(Alienseed.BaseNetworkServer.Accounting.IUserClient client)
        {
            Packet.WriteUserLogin(Writer, client);
        }

        public void OnUserLogout(Alienseed.BaseNetworkServer.Accounting.IUserClient client)
        {
            Packet.WriteUserLogout(Writer, client);
        }

        public void OnChat(Alienseed.BaseNetworkServer.INetState user, string text)
        {
            Packet.WriteChat(Writer, user.User, text);
        }

        public void OnAudio(Guid userID, Audio.Codecs.CodecID Codec, byte[] data, int dataLen)
        {
            Packet.WriteAudioData(Writer, userID, Codec, data, dataLen);
        }
    }
}
