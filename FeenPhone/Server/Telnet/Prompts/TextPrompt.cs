using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alienseed.BaseNetworkServer.Network.Telnet.Prompts
{
    abstract class TextPrompt : BaseTextPrompt
    {
        public abstract BaseTextPrompt OnResponse(TelNetState client, string text, bool cancel);
        public sealed override BaseTextPrompt OnResponse(BaseTelNetState client, string text, bool cancel)
        {
            if (client is TelNetState)
                return OnResponse((TelNetState)client, text, cancel);
            return null;
        }

    }
}
