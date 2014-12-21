using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeenPhone
{
    static class Settings
    {

        public static event EventHandler AppClosing;


        public static void InvokeAppClosing(object sender)
        {
            if (AppClosing != null)
                AppClosing(sender, null);
        }

        public static Properties.Settings Container { get { return Properties.Settings.Default; } }


    }
}
