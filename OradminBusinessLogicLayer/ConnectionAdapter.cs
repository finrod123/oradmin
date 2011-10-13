using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using myentitylibrary;

namespace oradminbl
{
    using ConnectionKey = String;

    public class ConnectionAdapter : EntityDataAdapter<Connection, ConnectionData, ConnectionKey>
    {
        #region Members
        string connectionsFileName;
        XmlSerializer serializer = new XmlSerializer(typeof(ConnectionData),
                                                     new XmlRootAttribute("Connections"));
        #endregion

        #region Constructor
        public ConnectionAdapter(string connectionsFileName)
        {
            if (!File.Exists(connectionsFileName))
                throw new FileNotFoundException("Connections file not found", connectionsFileName);

            this.connectionsFileName = connectionsFileName;
        }
        #endregion

        public override bool GetChanges(Connection entity, out ConnectionData data)
        {
            throw new NotImplementedException();
        }
        public override bool GetChanges(IEnumerable<Connection> entities, out IEnumerable<ConnectionData> data)
        {
            throw new NotImplementedException();
        }
        public override bool GetChanges(out IEnumerable<ConnectionData> data)
        {
            // access a file and read all connections
            XmlTextReader reader = new XmlTextReader(new FileStream(this.connectionsFileName, FileMode.Open));

            if (!this.serializer.CanDeserialize(reader))
            {
                data = null;
                return false;
            }

            try
            {
                data = this.serializer.Deserialize(reader) as IEnumerable<ConnectionData>;
                if (data == null)
                    return false;
            }
            catch (InvalidOperationException e)
            {
                throw new DataReadException("Cannot read connections from a file", e);
            }
            finally
            {
                reader.Close();
            }

            return true;
        }
    }
}