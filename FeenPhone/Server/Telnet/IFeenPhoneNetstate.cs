using Alienseed.BaseNetworkServer.Accounting;
using Alienseed.BaseNetworkServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace FeenPhone.Server
{
    internal interface IFeenPhoneNetState : IFeenPhoneEvents, INetState
    {
        bool Login(string username, string password);
        void OnLoginSuccess();
        void OnLoginFailed();
        Alienseed.BaseNetworkServer.PacketServer.IPacketWriter Writer { get; }
    }

    interface IFeenPhoneEvents
    {
        void OnUserConnected(INetState user);
        void OnUserDisconnected(INetState user);

        void OnUserLogin(IUserClient client);
        void OnUserLogout(IUserClient client);

        void OnChat(INetState user, string text);

        void OnAudio(Audio.Codecs.CodecID Codec, byte[] data, int dataLen);
    }
}