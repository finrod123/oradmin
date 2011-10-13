namespace myentitylibrary
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
        Original = 1,
        Current = 2,
        Default = 4
    }

    public enum EDataVersion
    {
        Original = 1,
        Current = 2
    }
}