using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Data;
using System.Collections.Generic;
using myentitylibrary;

namespace oradminbl
{
    using ConnectionKey = String;
    /// <summary>
    /// Trida provadejici ukladani pripojeni
    /// </summary>
    public class ConnectionSaver :
        IEntityDataSaver<ConnectionCollection, Connection, ConnectionData, ConnectionKey>
    {
        #region Members
        string connectionsFileName;
        XmlSerializer serializer = new XmlSerializer(typeof(ConnectionData),
                                                     new XmlRootAttribute("Connections"));
        #endregion

        #region Constructor
        public ConnectionSaver(string connectionsFileName)
        {
            if (!File.Exists(connectionsFileName))
                throw new FileNotFoundException("Connections file not found!", connectionsFileName);

            this.connectionsFileName = connectionsFileName;
        }
        #endregion

        #region IEntityDataSaver<ConnectionManager,Connection,ConnectionData,string> Members
        /// <summary>
        /// Currently the only implemented saving method
        /// </summary>
        /// <param name="entityManager">Connection manager</param>
        public void SaveChanges(ConnectionCollection connections)
        {
            // open the connections file and truncate it
            XmlTextWriter writer = new XmlTextWriter(
                new FileStream(this.connectionsFileName, FileMode.Truncate), Encoding.Default);

            try
            {
                // serialize connections
                serializer.Serialize(writer,
                    connections.EntityView.SourceCollection as IEnumerable<Connection>);
            }
            catch (InvalidOperationException e)
            {
                throw new UpdateException("Connections cannot be serialized!", e);
            }
            finally
            {
                writer.Close();
            }
        }
        public void SaveChanges(IEnumerable<Connection> entities)
        {
            throw new NotImplementedException();
        }
        public void SaveChanges(Connection entity)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

}