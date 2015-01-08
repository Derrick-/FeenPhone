using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeenPhone.WPFApp.Models
{
    public class VersionInfoModel
    {
        public Version Version { get; private set; }
        public string VersionText { get; private set; }

        public VersionInfoModel()
        {
            this.Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            this.VersionText = Version.ToString();
        }
    }
}
