using System.IO;
using Avalonia.Controls;
using HLab.UI;

namespace HLab.Ui.Avalonia.FileDialogs;

public class AvaloniaSaveFileDialog(OpenFileDialog fileDialog) : WpfFileDialog(fileDialog), IOpenFileDialog
{
    public bool AllowMultiple { get; set; }
    public Stream OpenFile()
    {
        throw new System.NotImplementedException();
    }
}