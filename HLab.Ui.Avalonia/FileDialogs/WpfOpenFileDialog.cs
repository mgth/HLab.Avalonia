using System.IO;
using Avalonia.Controls;
using HLab.UI;

namespace HLab.Ui.Avalonia.FileDialogs;

public class WpfOpenFileDialog(OpenFileDialog fileDialog) : WpfFileDialog(fileDialog), IOpenFileDialog
{
    public bool AllowMultiple { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public Stream OpenFile()
    {
        throw new System.NotImplementedException();
    }

    //public Stream OpenFile() => fileDialog.;
}