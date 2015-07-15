using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeenPhone.WPFApp.Utils
{
    static class CommandArgs
    {
        static Dictionary<string, string> Args = new Dictionary<string, string>();

        static CommandArgs()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int index = 1; index < args.Length; index += 1)
            {
                string[] arg = args[index].Split(new char[] { '=' }, 2);
                if (arg.Any())
                {
                    arg[0] = arg[0].Trim().ToLowerInvariant();
                    if (arg[0].StartsWith("-"))
                        arg[0] = arg[0].Remove(0, 1);

                    Args[arg[0]] = arg.Length > 1 ? arg[1].Trim().ToLowerInvariant() : "";
                }
            }
        }

        public static bool HasArg(string arg)
        {
            return Args.ContainsKey(arg.ToLowerInvariant());
        }

        public static string GetArgValue(string arg)
        {
            if (!HasArg(arg))
                return null;
            return Args[arg.ToLowerInvariant()];
        }

    }
}
