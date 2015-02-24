using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FeenPhone
{
    public static class Validation
    {
        public const string ip4Regex = @"(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)";
        public const string ip4SingleRegex = @"^" + ip4Regex + "$";
        public const string ip4ListRegex = "(" + ip4Regex + @")(,\s*" + ip4Regex + ")*";

        public const string ip6Regex = @"(([A-Fa-f0-9]{1,4}::?){1,7}[A-Fa-f0-9]{1,4})";
        public const string ip6SingleRegex = @"^" + ip6Regex + "$";
        public const string ip6ListRegex = "(" + ip6Regex + @")(,\s*" + ip6Regex + ")*";

        public const string TorRegex = @"([_\-0-9a-z]{16}\.onion)";
        public const string TorSingleRegex = @"^" + TorRegex + "$";

        public const string HostnameRegex = @"((?=.{1,255}$)[0-9A-Za-z](?:(?:[0-9A-Za-z]|-){0,61}[0-9A-Za-z])?(?:\.[0-9A-Za-z](?:(?:[0-9A-Za-z]|-){0,61}[0-9A-Za-z])?)*\.?)";
        public const string HostnameSingleRegex = @"^" + HostnameRegex + "$";
        public const string HostnameListRegex = HostnameRegex + @"(,\s*" + HostnameRegex + ")*";

        private static Regex regexSingleHostname = new Regex(HostnameSingleRegex);
        public static bool IsValidHostnameOrIPAddress(string host)
        {
            return regexSingleHostname.IsMatch(host);
        }

        private static Regex regexSingleIP = new Regex(ip4SingleRegex);
        public static bool IsValidIPAddress(string host)
        {
            return regexSingleIP.IsMatch(host);
        }
    }
}
