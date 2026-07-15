using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using HLab.Icons.Avalonia;
using HLab.Icons.Avalonia.Icons;
using HLab.Mvvm.Annotations;

namespace HLab.UserNotification.Avalonia;

public class UserNotificationServiceAvalonia : IUserNotificationService
{
    // TrayIcon.IsVisible defaults to true, which makes its constructor immediately call
    // impl.SetIsVisible(true) → NIM_ADD with a transparent empty icon — a blink before we
    // have a real bitmap. Override the default to false so construction is a silent no-op
    // and the first NIM_ADD only fires when we intentionally show a real icon.
    private sealed class HiddenByDefaultTrayIcon : TrayIcon
    {
        static HiddenByDefaultTrayIcon()
        {
            IsVisibleProperty.OverrideDefaultValue<HiddenByDefaultTrayIcon>(false);
        }
    }

    // TODO : Toast implementation
    //readonly INotificationManager _manager;
    readonly TrayIcons _trayIcons = new();
    readonly HiddenByDefaultTrayIcon _trayIcon = new();
    readonly IIconService _icons;

    public UserNotificationServiceAvalonia(IIconService icons)
    {
        _icons = icons;
        _trayIcons.Add(_trayIcon);
        TrayIcon.SetIcons(Application.Current, _trayIcons);
        _trayIcon.Clicked += (o, a) => Click?.Invoke(o, a);

        // Avalonia's own ShutdownStarted handler (registered in TrayIcon's static ctor,
        // which ran before we get here) calls TrayIcon.Dispose() → NIM_DELETE. Our handler
        // fires second: PostMessage(WM_SIZE) forces the notification area to repaint so the
        // ghost icon vanishes immediately instead of lingering until hover.
        Dispatcher.UIThread.ShutdownStarted += (_, _) => RefreshNotificationArea();
    }

    public void AddMenu(int pos, NativeMenuItem item)
    {
        _trayIcon.Menu ??= new NativeMenu();

        if (pos < 0 || pos >= _trayIcon.Menu.Items.Count)
            _trayIcon.Menu.Items.Add(item);
        else
            _trayIcon.Menu.Items.Insert(pos, item);
    }

    public async Task AddMenuAsync(int pos, string header, string iconPath, Func<Task> action)
    {
        var icon = await GetImageAsync(iconPath, 32);

        NativeMenuItem item = new(header.ToString())
        {
            Icon = icon,
            //IsChecked = chk,
        };

        item.Click += async (o,a) => await action();

        AddMenu(pos, item);
    }

    public async Task AddMenuAsync(int pos, string header, string iconPath, ICommand command)
    {
        var icon = await GetImageAsync(iconPath, 256);

        NativeMenuItem item = new(header.ToString())
        {
            Icon = icon,
            Command = command,
            //IsChecked = chk,
        };

        AddMenu(pos, item);
    }

    async Task<Bitmap?> GetImageAsync(string path, int size)
    {
        if (await _icons.GetIconAsync(path) is not Control c) return null;

        if (Dispatcher.UIThread.CheckAccess())
            return XamlTools.GetBitmap(c, new(size, size));

        return await Dispatcher.UIThread.InvokeAsync(() => XamlTools.GetBitmap(c, new(size, size))) ;

    }

    public event Action<object?, object>? Click;

    public async Task SetIconAsync(string iconPath, int i)
    {
        // Daemon state callbacks (Running/Stopped/Dead/Paused) arrive on the socket receive
        // thread, but building the icon control is UI-thread-affine. Marshal and retry.
        if (!Dispatcher.UIThread.CheckAccess())
        {
            await await Dispatcher.UIThread.InvokeAsync<Task>(() => SetIconAsync(iconPath, i));
            return;
        }
        var icon = await GetImageAsync(iconPath, i);
        if(icon!=null)
        {
            using var memory = new MemoryStream();
            icon.Save(memory);

            memory.Position = 0;

            Icon = new WindowIcon(memory);
        }
    }

    public string ToolTipText
    {
        get => _toolTipText;
        set
        {
            _toolTipText = value;
            _trayIcon.ToolTipText = value;
        }
    }

    public WindowIcon? Icon
    {
        get => _icon;
        set
        {
            _icon = value;

            // Hold icon updates while hidden: on Win32 a NIM_MODIFY on a removed icon re-adds it,
            // which would defeat Visible=false. The latest icon is reapplied when shown again.
            if (!_visible) return;
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                // Re-check _visible at execution time: the lambda is queued before Visible=false
                // can run (they share the dispatcher queue), so the flag may have flipped.
                if (!_visible) return;
                // Set IsVisible BEFORE Icon. Avalonia's UpdateIcon calls NIM_ADD on the first
                // SetIcon call (_iconAdded=false); if we set IsVisible first, the NIM_ADD fires
                // with the empty transparent icon (Avalonia's GetOrCreateEmptyIcon) — invisible,
                // so Windows silently positions the slot. The subsequent SetIcon is NIM_MODIFY,
                // which updates the bitmap in-place at the already-correct position: no blink.
                if (!_trayIcon.IsVisible)
                    _trayIcon.IsVisible = true;
                _trayIcon.Icon = value;
            });
        }
    }
    WindowIcon? _icon;

    public bool Visible
    {
        get => _visible;
        set
        {
            _visible = value;
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (value)
                {
                    // Only show if we already have a bitmap; if not, the Icon setter will
                    // make the tray slot live once the first bitmap arrives.
                    if (_icon == null) return;
                    // IsVisible before Icon for the same reason as the Icon setter above:
                    // NIM_ADD fires on SetIcon when _iconAdded=false; doing it here first
                    // with the transparent empty icon keeps the real bitmap at NIM_MODIFY.
                    _trayIcon.IsVisible = true;
                    _trayIcon.Icon = _icon;
                }
                else
                {
                    _trayIcon.IsVisible = false;
                    // Windows doesn't repaint the notification area after NIM_DELETE until
                    // the user hovers. Posting WM_SIZE to TrayNotifyWnd forces an immediate
                    // redraw so the ghost icon vanishes right away.
                    RefreshNotificationArea();
                }
            });
        }
    }
    bool _visible = true;

    string _toolTipText;

    public void Show()
    {
        /*
        _trayIcon = new TrayIcon
        {
            Icon = Icon,
            ToolTipText = ToolTipText,
                
        };
        */

    }

    public void Notify(string title, string message)
    {
        //_manager.ShowNotification(new Notification { Title = title, Body = message});
        //_taskbarIcon.ShowNotification(title, message, NotificationIcon.None);
    }
    public void NotifyInfo(string title, string message)
    {
        //_manager.ShowNotification(new Notification { Title = title, Body = message});
        //_taskbarIcon.ShowNotification(title, message, NotificationIcon.Info);
    }
    public void NotifyWarning(string title, string message)
    {
        //_manager.ShowNotification(new Notification { Title = title, Body = message});
        //_taskbarIcon.ShowNotification(title, message, NotificationIcon.Warning);
    }
    public void NotifyError(string title, string message)
    {
        //_manager.ShowNotification(new Notification { Title = title, Body = message});
        //_taskbarIcon.ShowNotification(title, message, NotificationIcon.Error);
    }

    // After any Shell_NotifyIcon call, Windows does not repaint the notification area until
    // the user hovers. Sending WM_SIZE to TrayNotifyWnd forces an immediate redraw.
    static void RefreshNotificationArea()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
        RefreshNotificationAreaWin32();
    }

    [SupportedOSPlatform("windows")]
    static void RefreshNotificationAreaWin32()
    {
        var hTray = FindWindow("Shell_TrayWnd", null);
        if (hTray == IntPtr.Zero) return;
        var hChild = FindWindowEx(hTray, IntPtr.Zero, "TrayNotifyWnd", null);
        if (hChild != IntPtr.Zero)
            PostMessage(hChild, WM_SIZE, IntPtr.Zero, IntPtr.Zero);
    }

    const uint WM_SIZE = 0x0005;

    [DllImport("user32.dll", SetLastError = true)]
    [SupportedOSPlatform("windows")]
    static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    [SupportedOSPlatform("windows")]
    static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string? lpszWindow);

    [DllImport("user32.dll")]
    [SupportedOSPlatform("windows")]
    static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

}