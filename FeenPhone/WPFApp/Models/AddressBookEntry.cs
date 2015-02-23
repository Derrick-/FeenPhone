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
        public string IP { get; set; }
        public int Port { get; set; }
        public string Address
        {
            get { return string.Format("{0}:{1}", IP, Port); }
            set
            {
                string ip;
                int port;
                if (!TryGetAddressAndPort(value, out ip, out port))
                    throw new ArgumentException("Address must be in the format X.X.X.X[:port]", "Address");
                IP = ip;
                Port = port;

            }
        }

        public AddressBookEntry() { }

        public AddressBookEntry(string IpAndPortString, string name = null)
        {
            if(name!=null)
            this.Name = name.Trim();
            Address = IpAndPortString;
        }

        public AddressBookEntry(IPAddress ip, int port, string name = null)
        {
            this.Name = name;
            this.IP = ip.ToString();
            this.Port = port;
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

            IPAddress ip;
            if (IPAddress.TryParse(parts[0].Trim(), out ip))
                address = ip.ToString();
            else
                return false;

            if (parts.Length > 1)
            {
                if (!int.TryParse(parts[1].Trim(), out port))
                    return false;
            }
            return true;
        }

    }
}
