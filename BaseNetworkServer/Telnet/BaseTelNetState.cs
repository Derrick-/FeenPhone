using Alienseed.BaseNetworkServer.Telnet.Prompts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace  Alienseed.BaseNetworkServer.Telnet
{
    public abstract class BaseTelNetState : TCPNetState<NetworkTextReader, NetworkTextWriter>
    {
        protected override string LogTitle { get { return "Telnet"; } }

        public abstract string WelcomeMessage { get; }
        public abstract BaseTextPrompt CreateFirstPrompt();

        protected virtual int screenWidth { get { return 80; } }

        public BaseTelNetState(System.Net.Sockets.NetworkStream stream, IPEndPoint ep) : base(stream, ep, 1024)
        {
            Reader.OnTextLine += Reader_OnText;
            Reader.PreviewIncoming += Reader_PreviewIncoming;
            CurrentPrompt = CreateFirstPrompt();
        }

        #region Prompt Handling

        protected void SendInfoLine(string format, params object[] args)
        {
            SendInfoLine(string.Format(format, args));
        }

        protected void SendInfoLine(string line)
        {
            Writer.WriteLine("\r{0}{1}", line, new string(' ', Math.Max(0, screenWidth - line.Length)));
            RefreshPrompt();
        }

        protected void RefreshPrompt()
        {
            CurrentPrompt.SendTo(Writer);
            Writer.Write(Reader.CurrentBuffer);
        }

        public void Reader_OnText(string text)
        {
            if (CurrentPrompt != null)
                CurrentPrompt = CurrentPrompt.OnResponse(this, text, false);
        }

        protected BaseTextPrompt _CurrentPrompt = null;
        protected BaseTextPrompt CurrentPrompt
        {
            get { return _CurrentPrompt; }
            set
            {
                if (_CurrentPrompt != value)
                {
                    if (value == null)
                        Reader.OnChar -= Reader_OnChar;
                    else if (_CurrentPrompt == null)
                        Reader.OnChar += Reader_OnChar;
                    _CurrentPrompt = value;
                }
                if (_CurrentPrompt != null)
                {
                    Reader.ClearBuffer();
                    _CurrentPrompt.SendTo(Writer);
                }
            }
        }

        const char ESC = (char)27;
        void Reader_OnChar(NetworkTextReader.OnReadCharArgs args)
        {
            if (args.Read == ESC && CurrentPrompt != null)
            {
                args.Handled = true;
                Write((char)0);
                WriteLine();
                CurrentPrompt = CurrentPrompt.OnResponse(this, null, true);
            }
        }

        #endregion

        #region ConnectionState

        public bool Echo { get; set; }

        protected override void OnConnected()
        {
            if (!string.IsNullOrEmpty(WelcomeMessage))
                Writer.WriteLine(WelcomeMessage);
            Writer.Write(TelnetControlCodes.IAC_WILL_ECHO);
        }

        #endregion

        #region Reading

        public override void Dispose()
        {
            base.Dispose();
        }

        internal void Reader_PreviewIncoming(ref byte[] bytes, ref int numbytes, ref bool handled)
        {
            for (int i = 0; i < numbytes; i++)
            {
                while (i < numbytes && (bytes[i] == TelnetControlCodes.IAC || bytes[i] == NetworkTextReader.ETX))
                {
                    if (bytes[i] == TelnetControlCodes.IAC)
                    {
                        int toRemove = ParseControlCharacters(bytes.Skip(i).Take(numbytes - i));
                        Console.Write("Codes: ");
                        for (int j = i; j < i + toRemove; j++)
                        {
                            Console.Write(bytes[j].ToString() + " ");
                        }
                        Console.WriteLine();
                        RemoveBytes(ref bytes, ref numbytes, i, toRemove);
                    }

                    if (bytes[i] == NetworkTextReader.ETX)
                    {
                        int toRemove = ParseControlCharacters(bytes.Skip(i).Take(numbytes - i));
                        RemoveBytes(ref bytes, ref numbytes, 0, i+1);
                        OnCtrlC();
                    }
                }
            }

            if (numbytes > 0 && CurrentPrompt != null && Echo)
                DoEcho(bytes, numbytes);

        }

        protected virtual void OnCtrlC() { }

        private static void RemoveBytes(ref byte[] bytes, ref int numbytes, int start, int toRemove)
        {
            if (toRemove > 0)
            {
                for (int j = start; j < numbytes - toRemove; j++)
                    bytes[j] = bytes[j + toRemove];
                numbytes -= toRemove;
            }
        }

        private void DoEcho(byte[] bytes, int numbytes)
        {
            if (CurrentPrompt != null && CurrentPrompt.EchoChar != null)
            {
                byte[] newBytes = new byte[bytes.Length];
                for (int i = 0; i < numbytes; i++)
                {
                    if (!char.IsControl((char)bytes[i]))
                        newBytes[i] = (byte)CurrentPrompt.EchoChar.Value;
                    else
                        newBytes[i] = bytes[i];
                }
                Writer.Write(newBytes, numbytes);
            }
            else
                Writer.Write(bytes, numbytes);
        }

        private int ParseControlCharacters(IEnumerable<byte> bytes)
        {
            int num = 0;
            if (bytes.Count() > 0)
                if (bytes.FirstOrDefault() == TelnetControlCodes.IAC)
                {
                    num = 1;
                    if (bytes.Count() > 0)
                    {
                        byte cmd = bytes.Skip(num).FirstOrDefault();
                        switch (cmd)
                        {

                            case TelnetControlCodes.SB:
                                {
                                    while (bytes.Count() > 0)
                                    {
                                        if (bytes.Skip(num++).FirstOrDefault() == TelnetControlCodes.SE || num > bytes.Count())
                                            break;
                                    }
                                    break;
                                }

                            case TelnetControlCodes.AYT:
                                num++;
                                Writer.Write("Hi!");
                                break;

                            case TelnetControlCodes.WILL:
                                num++;
                                if (bytes.Count() > 0)
                                {
                                    byte opt = bytes.Skip(num++).FirstOrDefault();
                                    switch (opt)
                                    {
                                        case TelnetControlCodes.SuppressGA: Writer.Write(new byte[] { TelnetControlCodes.IAC, TelnetControlCodes.DO, TelnetControlCodes.SuppressGA }); break;
                                        default: Writer.Write(new byte[] { TelnetControlCodes.IAC, TelnetControlCodes.DONT, opt }); break;
                                    }
                                }
                                break;

                            case TelnetControlCodes.WONT:
                                num++;
                                if (bytes.Count() > 0)
                                    num++;
                                break;

                            case TelnetControlCodes.DO:
                                num++;
                                if (bytes.Count() > 0)
                                {
                                    byte opt = bytes.Skip(num++).FirstOrDefault();
                                    switch (opt)
                                    {
                                        case TelnetControlCodes.Echo: Echo = true; break;
                                        case TelnetControlCodes.SuppressGA: Writer.Write(new byte[] { TelnetControlCodes.IAC, TelnetControlCodes.WILL, TelnetControlCodes.SuppressGA }); break;
                                        default: Writer.Write(new byte[] { TelnetControlCodes.IAC, TelnetControlCodes.WONT, opt }); break;
                                    }
                                }
                                break;
                            case TelnetControlCodes.DONT:
                                num++;
                                if (bytes.Count() > 0)
                                {
                                    byte opt = bytes.Skip(num++).FirstOrDefault();
                                    switch (opt)
                                    {
                                        case TelnetControlCodes.Echo: Echo = false; break;
                                        case TelnetControlCodes.SuppressGA: Writer.Write(new byte[] { TelnetControlCodes.IAC, TelnetControlCodes.WILL, TelnetControlCodes.SuppressGA }); break;
                                    }
                                }
                                break;

                            case TelnetControlCodes.BRK: goto case TelnetControlCodes.NOP;
                            case TelnetControlCodes.SE: goto case TelnetControlCodes.NOP;
                            case TelnetControlCodes.DM: goto case TelnetControlCodes.NOP;
                            case TelnetControlCodes.IP: goto case TelnetControlCodes.NOP;
                            case TelnetControlCodes.AO: goto case TelnetControlCodes.NOP;
                            case TelnetControlCodes.EC: goto case TelnetControlCodes.NOP;
                            case TelnetControlCodes.EL: goto case TelnetControlCodes.NOP;
                            case TelnetControlCodes.GA: goto case TelnetControlCodes.NOP;

                            case TelnetControlCodes.NOP:
                                {
                                    num += 1;
                                    break;
                                }

                        }
                    }
                }
            return num;
        }


        #endregion

        #region Writing

        public void WriteLine()
        {
            if (Writer != null) Writer.WriteLine();
        }

        public void WriteLine(string text)
        {
            if (Writer != null) Writer.WriteLine(text);
        }

        public void Write(char c)
        {
            if (Writer != null) Writer.Write(c);
        }

        public void WriteLine(string p, params object[] args)
        {
            if (Writer != null) Writer.WriteLine(p, args);
        }

        #endregion
    }
}

public static class TelnetControlCodes
{
    public const byte SE = 240;
    public const byte NOP = 241;
    public const byte DM = 242;
    public const byte BRK = 243;
    public const byte IP = 244;
    public const byte AO = 245;
    public const byte AYT = 246;
    public const byte EC = 247;
    public const byte EL = 248;
    public const byte GA = 249;
    public const byte SB = 250;
    public const byte WILL = 251;
    public const byte WONT = 252;
    public const byte DO = 253;
    public const byte DONT = 254;
    public const byte IAC = 255;
    
    public const byte Echo = 1;
    public const byte SuppressGA = 3;

    public static byte[] IAC_WILL_ECHO = new byte[] { IAC, WILL, Echo };

}