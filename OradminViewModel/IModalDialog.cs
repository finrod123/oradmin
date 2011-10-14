using System;
using System.Windows;

namespace oradminviewmodel
{
    public interface IModalDialog
    {
        IModalView Content { get; }
        void ShowDialog();
        bool? DialogResult { get; }
    }
}