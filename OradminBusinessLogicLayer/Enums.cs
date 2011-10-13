namespace oradminbl
{
    #region Connection enums

    public enum EConnectionError
    {
        EmptyName,
        EmptyUserName,
        EmptyTnsName,
        DuplicateName,
        InvalidPrivileges,
        InvalidNamingMethod,
        InvalidConnectDescriptor
    }

    public enum EDbaPrivileges
    {
        Normal,
        SYSDBA,
        SYSOPER
    }

    public enum ENamingMethod
    {
        ConnectDescriptor,
        TnsNaming
    }

    public enum EServerType
    {
        Dedicated,
        Shared,
        Pooled
    }

    public enum EProtocolType
    {
        Tcp,
        Ipc
    }

    #endregion
}