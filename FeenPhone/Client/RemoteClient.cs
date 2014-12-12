using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone.Client
{
    class RemoteClient : BaseClient
    {
        public override bool IsConnected
        {
            get { throw new NotImplementedException(); }
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        internal override void SendChat(string text)
        {
            throw new NotImplementedException();
        }
    }
}
