using Avalonia.Threading;
using HLab.UI;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.LogicalTree;
using HLab.Ui.Avalonia.FileDialogs;

namespace HLab.Ui.Avalonia;

public class UiAvaloniaImplementation : IUiPlatformImplementation
{
    public static void Initialize()
    {
        UiPlatform.Configure(new UiAvaloniaImplementation());
    }

    static TopLevel? MainTopLevel
        => Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: { } w } ? w : null;

    public IOpenFileDialog CreateOpenFileDialog() => new AvaloniaOpenFileDialog();

    public ISaveFileDialog CreateSaveFileDialog() => new AvaloniaSaveFileDialog();

    public IEnumerable GetLogicalChildren(object fe)
    {
        if (fe is ILogical logical) return logical.LogicalChildren;
        return Array.Empty<object>();
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

    public IGuiTimer CreateGuiTimer() => new GuiTimer();

    public string? GetClipboardText()
    {
        var clipboard = MainTopLevel?.Clipboard;
        if (clipboard is null) return null;
        return clipboard.TryGetTextAsync().GetAwaiter().GetResult();
    }

    public void SetClipboardText(string text)
    {
        var clipboard = MainTopLevel?.Clipboard;
        clipboard?.SetTextAsync(text);
    }

    public void Quit()
    {
        Dispatcher.UIThread.BeginInvokeShutdown(DispatcherPriority.Normal);
    }
}
