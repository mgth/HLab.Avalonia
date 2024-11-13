using Avalonia.Threading;
using HLab.UI;
using System;
using System.Collections;
using System.Threading.Tasks;
using Avalonia;

namespace HLab.Ui.Avalonia;

public class UiAvaloniaImplementation : IUiPlatformImplementation
{
    public static void Initialize()
    {
        UiPlatform.Configure(new UiAvaloniaImplementation());
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
    public async Task InvokeOnUiThreadAsync(Func<Task> callback)
    {
        await Dispatcher.UIThread.InvokeAsync(callback);
    }

    public void VerifyAccess() => Dispatcher.UIThread.VerifyAccess();

    public IGuiTimer CreateGuiTimer()
    {
        throw new NotImplementedException();
    }

    public string? GetClipboardText()
    {
        throw new NotImplementedException();
    }

    public void SetClipboardText(string text)
    {
        throw new NotImplementedException();
    }

    public void Quit()
    {
        Dispatcher.UIThread.BeginInvokeShutdown(DispatcherPriority.Normal);
    }
}
