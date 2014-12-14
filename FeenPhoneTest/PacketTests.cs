using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Alienseed.BaseNetworkServer.PacketServer;
using FeenPhone;
using Alienseed.BaseNetworkServer.Accounting;
using System.Collections.Generic;
using FeenPhone.Accounting;
using System.IO;
using FeenPhone.Client;

namespace FeenPhoneTest
{
    [TestClass]
    public class PacketTests
    {

        List<IUser> LastUsersListEventData = null;

        [TestMethod]
        public void UsersListTest()
        {
            NetworkPacketWriter writer = new NetworkPacketWriter();

            List<IUser> UsersList = new List<IUser>();

            for (int i = 0; i < 10; i++)
                UsersList.Add(new MockAccount());

            MemoryStream ms = new MemoryStream();
            writer.SetStream(ms);

            Packet.WriteUserList(writer, UsersList);

            ClientPacketHandler handler = new ClientPacketHandler();

            EventSource.OnUserList += EventSource_OnUserList;
            LastUsersListEventData = null;
            handler.Handle(new Queue<byte>(ms.ToArray()));
            EventSource.OnUserList += EventSource_OnUserList;

            Assert.IsNotNull(LastUsersListEventData);
            Assert.AreEqual(UsersList.Count, LastUsersListEventData.Count);

            for (int i = 0; i < UsersList.Count; i++)
            {
                Assert.AreEqual(UsersList[i].ID, LastUsersListEventData[i].ID);
                Assert.AreEqual(UsersList[i].Username, LastUsersListEventData[i].Username);
                Assert.AreEqual(UsersList[i].Nickname, LastUsersListEventData[i].Nickname);
                Assert.AreEqual(UsersList[i].IsAdmin, LastUsersListEventData[i].IsAdmin);
            }
        }

        void EventSource_OnUserList(object sender, UserListEventArgs e)
        {
            LastUsersListEventData = new List<IUser>(e.Users);
        }

        [TestMethod]
        public void AudioPacketTest()
        {
            NetworkPacketWriter writer = new NetworkPacketWriter();
            MemoryStream ms = new MemoryStream();
            writer.SetStream(ms);

            byte[] audioData = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            Packet.WriteAudioData(writer, FeenPhone.Audio.Codecs.CodecID.Gsm610ChatCodec, audioData, 10);

            ClientPacketHandler handler = new ClientPacketHandler();

            EventSource.OnAudioData += EventSource_OnAudioData;
            LastAudioDataEventArgs = null;
            handler.Handle(new Queue<byte>(ms.ToArray()));
            EventSource.OnAudioData += EventSource_OnAudioData;

            Assert.IsNotNull(LastAudioDataEventArgs);
            Assert.AreEqual(FeenPhone.Audio.Codecs.CodecID.Gsm610ChatCodec, LastAudioDataEventArgs.Codec);
            Assert.AreEqual(10, LastAudioDataEventArgs.Data.Length);
            Assert.AreEqual(10, LastAudioDataEventArgs.DataLen);
        }

        AudioDataEventArgs LastAudioDataEventArgs = null;
        private void EventSource_OnAudioData(object sender, AudioDataEventArgs e)
        {
            LastAudioDataEventArgs = e;
        }
    }
}
