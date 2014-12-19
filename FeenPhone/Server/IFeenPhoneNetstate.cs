using Alienseed.BaseNetworkServer.Accounting;
using Alienseed.BaseNetworkServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace FeenPhone.Server
{
    internal interface IFeenPhonePacketNetState : IFeenPhoneNetState
     {
        Alienseed.BaseNetworkServer.PacketServer.IPacketWriter Writer { get; }
     }

    internal interface IFeenPhoneNetState : IFeenPhoneClient, INetState, System.ComponentModel.INotifyPropertyChanged
    {
        ushort LastPing { get; set; }

        bool Login(string username, string password);
        void LoginSuccess();
        void LoginFailed();
    }

    interface IFeenPhoneClient
    {
        void OnUserConnected(INetState user);
        void OnUserDisconnected(INetState user);

        void OnUserLogin(IUserClient client);
        void OnUserLogout(IUserClient client);

        void OnChat(INetState user, string text);

        void OnAudio(Audio.Codecs.CodecID Codec, byte[] data, int dataLen);
    }
}