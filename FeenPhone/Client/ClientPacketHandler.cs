using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone.Client
{
    class ClientPacketHandler : BasePacketHandler
    {
        protected override void UserLogin(IUser user)
        {
            EventSource.InvokeOnUserConnected(this, user);
        }

        protected override void UserLogout(IUser user)
        {
            EventSource.InvokeOnUserDisconnected(this, user);
        }

        protected override void OnChat(IUser user, string text)
        {
            EventSource.InvokeOnChat(this, user, text);
        }

        protected override void OnAudio(Guid userID, Audio.Codecs.CodecID Codec, byte[] data, int dataLen)
        {
            EventSource.InvokeOnAudio(this, userID, Codec, data, dataLen);
        }

        protected override void OnPingReq(ushort timestamp)
        {
            EventSource.InvokeOnPingReq(this, timestamp);
        }

        protected override void OnPingResp(ushort elapsed)
        {
            FeenPhone.Client.EventSource.InvokeOnPingResp(this, elapsed);
        }

        protected override void OnLoginStatus(bool isLoggedIn, ushort version, string message)
        {
            EventSource.InvokeOnLoginStatus(this, isLoggedIn, version, message);
        }

        protected override void LoginInfo(string username, string password)
        {
            Console.WriteLine("Invalid client packet LoginInfo received.");
        }

        protected override void OnUserList(IEnumerable<IUser> users)
        {
            EventSource.InvokeOnUserList(this, users);
        }

        protected override IUser GetUserObject(Guid id, bool isadmin, string username, string nickname)
        {
            return UserRepo.CreateOrUpdateUser(id, isadmin, username, nickname);
        }
    }
}

