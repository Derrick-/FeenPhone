
namespace Alienseed.BaseNetworkServer.Network.Telnet.Prompts
{
    public abstract class BaseTextPrompt
    {
        public virtual char? EchoChar { get { return null; } }

        protected abstract string[] QuestionText { get; }

        public void SendTo(BaseTelNetState ns)
        {
            SendTo(ns.Writer);
        }

        public void SendTo(NetworkTextWriter writer)
        {
            if (QuestionText.Length == 1)
                   writer.Write(QuestionText[0]);
            else
            {
                foreach (string line in QuestionText)
                    writer.WriteLine(line);
                writer.Write("> ");
            }
        }

        public abstract BaseTextPrompt OnResponse(BaseTelNetState client, string text, bool cancel);
    }
}
