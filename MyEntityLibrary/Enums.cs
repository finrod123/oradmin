namespace oradmin
{
    public enum EEntityState
    {
        Unchanged,
        Added,
        Modified,
        Deleted,
        Detached
    }

    public enum EDataVersionWithDefault
    {
        Original,
        Current,
        Default
    }

    public enum EDataVersion
    {
        Original,
        Current
    }
}