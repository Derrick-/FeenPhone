using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone.Client
{
    class LocalClient : BaseClient
    {

        public override bool IsConnected
        {
            get { return true; }
        }

    }
}
