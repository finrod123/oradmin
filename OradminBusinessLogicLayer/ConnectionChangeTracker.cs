using System;
using myentitylibrary;

namespace oradminbl
{
    using ConnectionKey = String;

    public class ConnectionChangeTracker : EntityChangeTracker<Connection, ConnectionData, ConnectionKey>
    {

        protected override void createVersionedFields()
        {
            throw new NotImplementedException();
        }

        protected override void readInitialData()
        {
            throw new NotImplementedException();
        }

        protected override bool readChanges()
        {
            throw new NotImplementedException();
        }

        public override void RejectChanges()
        {
            throw new NotImplementedException();
        }

        public override void AcceptChanges()
        {
            throw new NotImplementedException();
        }

        public override bool Merge(ConnectionData data, EMergeOptions mergeOptions)
        {
            throw new NotImplementedException();
        }
    }
}