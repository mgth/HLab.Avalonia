using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Threading;
using HLab.Base.Avalonia.DependencyHelpers;
using HLab.Mvvm.Annotations;

namespace HLab.Mvvm.Avalonia;

using H = DependencyHelper<DefaultWindow>;

/// <summary>
/// Logique d'interaction pour DefaultWindow.xaml
/// </summary>
public partial class DefaultWindow : Window, IWindow
{
    public DefaultWindow()
    {
        InitializeComponent();

        // The backdrop tint is computed from theme resources: recompute it when
        // the variant flips (system dark/light change) — resources looked up in
        // code don't re-resolve on their own, unlike DynamicResource.
        ActualThemeVariantChanged += (_, _) => UpdateBackdrop();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        UpdateBackdrop();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ActualTransparencyLevelProperty) UpdateBackdrop();
    }

    /// <summary>
    /// The hint asks for Mica / AcrylicBlur / Blur in that order; what the platform grants
    /// decides the background. Windows Mica/Acrylic draw their own backdrop behind a fully
    /// transparent window. X11 Blur (KWin) only blurs what is behind: an acrylic look needs
    /// a translucent tint on top of it. Anything less would leave the window see-through,
    /// so without any compositor effect the background falls back to opaque.
    /// </summary>
    void UpdateBackdrop()
    {
        var level = ActualTransparencyLevel;

        if (level == WindowTransparencyLevel.Mica || level == WindowTransparencyLevel.AcrylicBlur)
        {
            Background = Brushes.Transparent;
            return;
        }

        var color = ThemeBackgroundColor();
        Background = new SolidColorBrush(
            level == WindowTransparencyLevel.Blur
                ? new Color(0xA8, color.R, color.G, color.B)
                : new Color(0xFF, color.R, color.G, color.B));
    }

    Color ThemeBackgroundColor()
    {
        if (this.TryFindResource("ThemeBackgroundColor", ActualThemeVariant, out var value)
            && value is Color themed && themed.A > 0)
            return themed;

        if (this.TryFindResource("HLab.Colors.Background", ActualThemeVariant, out value)
            && value is Color hlab)
            return hlab;

        return Color.FromRgb(0x20, 0x20, 0x20);
    }

    public void SetOwner(IView owner)
    {
       if (owner is Visual v && TopLevel.GetTopLevel(v) is Window w) _owner = w;
    }
    Window? _owner;

    /// <summary>
    /// Dialog result reported by IWindow.ShowDialog : Avalonia's own dialog result
    /// (Window.Close(object)) is not publicly readable, so views set this instead.
    /// </summary>
    public bool? DialogResult { get; set; }

    /// <summary>
    /// Blocking modal semantics expected by IWindow consumers (WPF heritage) :
    /// shows the window and pumps a nested dispatcher frame until it closes.
    /// </summary>
    public bool? ShowDialog()
    {
       var frame = new DispatcherFrame();
       Closed += (_, _) => frame.Continue = false;

       // Sans propriétaire un dialogue n'est pas modal et passe derrière au
       // premier clic : à défaut d'owner explicite, prendre la fenêtre active
       // (au login il n'y en a pas encore → Show simple, comme avant).
       var owner = _owner ?? ActiveWindow();

       if (owner is not null) ShowDialog(owner);
       else Show();

       Dispatcher.UIThread.PushFrame(frame);
       return DialogResult;
    }

    Window? ActiveWindow()
    {
       if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
          return null;

       var owner = desktop.Windows.FirstOrDefault(w => w.IsActive && !ReferenceEquals(w, this))
                   ?? desktop.MainWindow;

       return ReferenceEquals(owner, this) ? null : owner;
    }

    public IView? View
    {
        get => GetValue(ViewProperty);
        set => SetValue(ViewProperty, value);
    }

    public static readonly StyledProperty<IView> ViewProperty =
        H.Property<IView>()
            .OnChanged((w,e) =>
            {
                w.ContentControl.Content = e.NewValue.Value;
            })
            .Register();

}
