using System.IO;
using HLab.UI;

namespace HLab.Ui.Avalonia.FileDialogs;

public class AvaloniaOpenFileDialog : AvaloniaFileDialog, IOpenFileDialog
{
    public bool AllowMultiple { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public Stream OpenFile()
    {
        throw new System.NotImplementedException();
    }
}
