using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace  Alienseed.BaseNetworkServer.Telnet
{
    public class NetworkTextReader : BaseStreamReader
    {
        public delegate void OnReadLineHandler(string text);
        public event OnReadLineHandler OnTextLine;

        public class OnReadCharArgs
        {
            public char Read { get; private set; }
            public bool Handled { get; set; }

            public OnReadCharArgs(char read)
            {
                Read = read;
                Handled = false;
            }
        }

        public delegate void OnReadCharHandler(OnReadCharArgs args);
        public event OnReadCharHandler OnChar;

        StringBuilder inText = new StringBuilder();

        public string CurrentBuffer { get { return inText.ToString(); } }

        internal const char ETX = (char)0x03;

        internal const char BS = (char)0x08;
        internal const char LF = (char)0x0A;
        internal const char CR = (char)0x0D;

        internal void ClearBuffer()
        {
            inText.Clear();
        }

        protected override void OnRead()
        {
            ProcessOnCharEvent();
            if (InStream.Count > 0)
            {
                byte[] bytes = new byte[InStream.Count];

                InStream.CopyTo(bytes, 0);
                string read = Encoding.ASCII.GetString(bytes);

                bool parseBS = read.Contains(BS);

                inText.Append(read);

                ResolveBackspaces(inText);

                InStream.Clear();
                string line = null;
                do
                {
                    line = GetLineTrimmed();
                    if (line != null)
                        InvokeOnText(line);
                } while (line != null);
            }
        }

        private void ProcessOnCharEvent()
        {
            if (OnChar != null)
            {
                int count = InStream.Count;
                for (int i = 0; i < count; i++)
                {
                    byte b = InStream.Dequeue();
                    char c = (char)b;
                    var args = new OnReadCharArgs(c);
                    OnChar(args);
                    if (!args.Handled)
                        InStream.Enqueue(b);
                }
            }
        }

        internal static void ResolveBackspaces(StringBuilder sb)
        {
            List<int> indexesToRemove = new List<int>();
            while (sb.Length > 0 && sb[0] == BS) sb.Remove(0, 1);
            for (int i = 0; i < sb.Length; i++)
            {
                if (sb[i] == BS)
                {
                    sb.Remove(i - 1, 2);
                    i -= 2;
                }
            }
        }

        private void InvokeOnText(string line)
        {
            if (OnTextLine != null) OnTextLine(line);
        }

        private string GetLineTrimmed()
        {
            int eol = -1;
            for (int i = 0; i < inText.Length; i++)
            {
                if (inText[i] == LF)
                {
                    eol = i;
                    break;
                }
            }
            if (eol > 0)
            {
                string line = inText.ToString(0, eol).Trim();
                while (eol < inText.Length && char.IsControl(inText[eol])) eol++;
                inText.Remove(0, eol);
                return line;
            }
            else return null;

        }

    }
}
