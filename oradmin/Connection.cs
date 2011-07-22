using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace oradmin
{
    public interface IConnection : IConnectDescriptor
    {
        int Id { get; }
        string Name { get; }
        string UserName { get; }
        EDbaPrivileges DbaPrivileges { get; }
        bool OsAuthenticate { get; }
        ENamingMethod NamingMethod { get; }
        string TnsName { get; }
    }
}