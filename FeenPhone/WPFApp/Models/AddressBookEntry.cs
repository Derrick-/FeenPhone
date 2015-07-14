using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FeenPhone.WPFApp.Models
{
    public class AddressBookEntry
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }

        public string Address
        {
            get { return string.Format("{0}:{1}", Host, Port); }
            set
            {
                string host;
                int port;
                if (!TryGetAddressAndPort(value, out host, out port))
                    throw new ArgumentException("Address must be in the format X.X.X.X[:port]", "Address");
                Host = host;
                Port = port;

            }
        }

        public AddressBookEntry() { }

        public AddressBookEntry(string IpAndPortString, string name = null, string password = null)
        {
            if (name != null)
                this.Name = name.Trim();
            Address = IpAndPortString;
            this.Password = password;
        }

        public AddressBookEntry(IPAddress ip, int port, string name = null, string password = null)
        {
            this.Name = name;
            this.Host = ip.ToString();
            this.Port = port;
            this.Password = password;
        }

        public static bool IsValidServerEntry(string server)
        {
            string address;
            int port;
            return TryGetAddressAndPort(server, out address, out port);
        }

        public static bool TryGetAddressAndPort(string value, out string address, out int port)
        {
            address = null;
            port = WPFApp.Controls.NetworkWPF.DefaultPort;

            string[] parts = value.Split(':');
            if (parts.Length < 1 || parts.Length > 2)
                return false;

            var host = parts[0].Trim();
            if (Validation.IsValidIPAddress(host))
            {
                IPAddress ip;
                if (!IPAddress.TryParse(host, out ip))
                    return false;
                address = ip.ToString();
            }
            else
            {
                if (!Validation.IsValidHostnameOrIPAddress(host))
                    return false;
                address = host;
            }

            if (parts.Length > 1)
            {
                if (!int.TryParse(parts[1].Trim(), out port))
                    return false;
            }
            return true;
        }

    }
}
