﻿namespace myentitylibrary
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
        bool Refresh();
    }

    public interface IUpdatableObject
    {
        void SaveChanges();
    }
}