using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Data;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace oradmin
{
    public class SessionViewManager
    {
        #region Members
        #region SQL SELECTS
        public const string ALL_VIEWS_NONTYPED_SELECT = @"
            SELECT
                owner, view_name,
                text, text_length
            FROM
                ALL_VIEWS
            WHERE
                type_text is null and
                type_text_length is null and
                oid_text is null and
                oid_text_length is null and
                view_type is null and
                view_type_owner is null and
                superview_name is null";
        #endregion
        
        SessionManager.Session session;
        OracleConnection conn;

        ObservableCollection<View> views = new ObservableCollection<View>();
        ListCollectionView view;
        #endregion

        #region Constructor
        public SessionViewManager(SessionManager.Session session)
        {
            if (session == null)
                throw new ArgumentNullException("Session");

            this.session = session;
            this.conn = this.session.Connection;

            view = new ListCollectionView(views);
        }
        #endregion

        #region Public interface
        public void Refresh()
        {

        }
        #endregion

        #region View class
        public class View : IEditableObject
        {
            #region Members
            #region SQL SELECTS
            public const string ALL_VIEWS_VIEW_SELECT = @"
                SELECT
                    owner, view_name,
                    text, text_length
                FROM
                    ALL_VIEWS
                WHERE
                    owner = :owner and
                    view_name = :view_name and
                    type_text is null and
                    type_text_length is null and
                    oid_text is null and
                    oid_text_length is null and
                    view_type is null and
                    view_type_owner is null and
                    superview_name is null";
            #endregion
            SessionManager.Session session;
            OracleConnection conn;

            ViewData data, copyData;
            #endregion

            #region Constructor
            public View(ViewData data, SessionManager.Session session)
            {
                if (session == null)
                    throw new ArgumentNullException("Session");

                this.session = session;
                this.conn = this.session.Connection;

                this.data = data;
            }
            #endregion

            #region Properties
            public string Owner
            {
                get { return this.data.owner; }
                set { this.data.owner = value; }
            }
            public string Name
            {
                get { return this.data.viewName; }
                set { this.data.viewName = value; }
            }
            public string Text
            {
                get { return this.data.text; }
                set { this.data.text = value;}
            }
            public int? TextLength
            {
                get { return this.data.textLength; }
                set { this.data.textLength = value; }
            }
            #endregion

            #region IEditableObject Members

            public void BeginEdit()
            {
                throw new NotImplementedException();
            }

            public void CancelEdit()
            {
                throw new NotImplementedException();
            }

            public void EndEdit()
            {
                throw new NotImplementedException();
            }

            #endregion
        }
	    #endregion

        #region View data struct
        public struct ViewData
        {
            #region Members
            public string owner;
            public string viewName;
            public string text;
            public int? textLength;
            #endregion

            #region Constructor
            public ViewData(
                string owner,
                string viewName,
                string text,
                int? textLength)
            {
                this.owner = owner;
                this.viewName = viewName;
                this.text = text;
                this.textLength = textLength;
            }
            #endregion
        }
        #endregion
    }
}
