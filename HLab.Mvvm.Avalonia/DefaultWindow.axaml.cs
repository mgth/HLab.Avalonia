using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
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
       throw new NotImplementedException();
    }

    public bool? ShowDialog()
    {
       throw new NotImplementedException();
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
