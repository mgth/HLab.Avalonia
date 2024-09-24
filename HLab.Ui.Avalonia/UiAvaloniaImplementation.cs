using Avalonia.Threading;
using HLab.UI;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace HLab.Ui.Avalonia;

public class UiAvaloniaImplementation : IUiPlatformImplementation
{
    public static void Initialize()
    {
        UiPlatform.Implementation = new UiAvaloniaImplementation();
    }

    public IOpenFileDialog CreateOpenFileDialog()
    {
        throw new NotImplementedException();
    }

    public ISaveFileDialog CreateSaveFileDialog()
    {
        throw new NotImplementedException();
    }

    public IEnumerable GetLogicalChildren(object fe)
    {
        throw new NotImplementedException();
    }

    public async Task InvokeOnUiThreadAsync(Action callback)
    {
        await Dispatcher.UIThread.InvokeAsync(callback);
    }
}
