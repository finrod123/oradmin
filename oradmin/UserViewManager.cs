using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace oradmin
{
    public class UserViewManager
    {
        #region Members
        #region SQL SELECTS
        public const string ALL_VIEWS_NONTYPED_SELECT_SCHEMA = @"
            SELECT
                owner, view_name,
                text, text_length
            FROM
                ALL_VIEWS
            WHERE
                owner = :owner and
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

        SessionViewManager sessionManager;
        ObservableCollection<SessionViewManager.View> views =
            new ObservableCollection<SessionViewManager.View>();
        #endregion
    }
}
