using System;
using myentitylibrary;

namespace oradminbl
{
    using ConnectionKey = String;

    public class ConnectionCollection : EntityCollection<Connection, ConnectionData, ConnectionKey>
    {
        protected override bool BelongsTo(Connection entity)
        {
            throw new NotImplementedException();
        }
    }
}