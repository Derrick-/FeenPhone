using System.Text;

namespace  Alienseed.BaseNetworkServer.Telnet
{
    public class NetworkTextWriter : BaseStreamWriter
    {
        public void Write(string text)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(text);
            Write(bytes);
        }

        public void Write(char c)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(c.ToString());
            Write(bytes);
        }
        public void Write(string format, params object[] args)
        {
            Write(string.Format(format, args));
        }

        public void WriteLine(string text)
        {
            Write(text);
            WriteLine();
        }

        public void WriteLine(string format, params object[] args)
        {
            WriteLine(string.Format(format, args));
        }

        internal static readonly byte[] CRLF = new byte[] { 0x0D, 0x0A };
        internal void WriteLine()
        {
            Write(CRLF);
        }
    }
}
