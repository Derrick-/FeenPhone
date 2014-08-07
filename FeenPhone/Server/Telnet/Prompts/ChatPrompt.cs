using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone.Server.Telnet.Prompts
{
    class ChatPrompt : TextPrompt
    {
        static string[] prompt = new string[] { "Chat> " };

        public override Alienseed.BaseNetworkServer.Telnet.Prompts.BaseTextPrompt OnResponse(TelNetState client, string text, bool cancel)
        {
            if(cancel)
                return new MainMenu(client.User.IsAdmin);
            EventSink.OnChat(client, text);
            return this;
        }

        protected override string[] QuestionText
        {
            get { return prompt; }
        }
    }
}
