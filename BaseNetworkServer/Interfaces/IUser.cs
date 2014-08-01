using System;
using System.Collections.Generic;

namespace Alienseed.BaseNetworkServer.Accounting
{
    public interface IUser : IEquatable<IUser>
    {
        string Username { get; }
        string Nickname { get; }

        bool IsAdmin { get; }

        Guid ID { get; }
    }
}
