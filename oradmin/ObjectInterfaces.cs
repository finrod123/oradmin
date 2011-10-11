namespace oradmin
{
    public interface IDeletableObject
    {
        void Delete();
    }

    public interface IUndeletableObject
    {
        void UnDelete();
    }

    public interface IEditableObjectInfo
    {
        bool IsEditing { get; }
    }

    public interface IRefreshableObject
    {
        void Refresh();
    }

    public interface IUpdatableObject
    {
        void SaveChanges();
    }
}