using System;
using Avalonia.Threading;
using HLab.UI;

namespace HLab.Ui.Avalonia;

public class GuiTimer : IGuiTimer
{
    readonly DispatcherTimer _timer;

    public GuiTimer()
    {
        _timer = new DispatcherTimer(DispatcherPriority.Background);
        _timer.Tick += _timer_Tick;
    }

    void _timer_Tick(object? sender, EventArgs e)
    {
        Tick?.Invoke(sender, e);
    }

    public void Start() => _timer.Start();
    public void Stop() => _timer.Stop();

    public void DoTick()
    {
        Dispatcher.UIThread.Post(() => Tick?.Invoke(this, EventArgs.Empty), DispatcherPriority.Background);
        _timer.Start();
    }

    public TimeSpan Interval
    {
        get => _timer.Interval;
        set => _timer.Interval = value;
    }

    public bool IsEnabled
    {
        get => _timer.IsEnabled;
        set => _timer.IsEnabled = value;
    }

    public event EventHandler? Tick;
}
