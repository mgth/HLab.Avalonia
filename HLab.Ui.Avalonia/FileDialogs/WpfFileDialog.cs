using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using HLab.UI;

namespace HLab.Ui.Avalonia.FileDialogs;

public class WpfFileDialog(FileDialog fileDialog) : IFileDialog
{

    public async Task Open(object parent) {
        if(parent is not Visual visual) return;
        var storage = TopLevel.GetTopLevel(visual).StorageProvider;

        var filePickerOptions = new FilePickerOpenOptions
        {
            Title = "Select a file",
        };

        var piker = await storage.OpenFilePickerAsync(filePickerOptions);
    }

    public void Reset()
    {
        throw new System.NotImplementedException();
    }

    public string FileName { get; set; }
    public string DefaultExt { get; set; }
    public string Filter { get; set; }
    public string? SuggestedFileName { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
}