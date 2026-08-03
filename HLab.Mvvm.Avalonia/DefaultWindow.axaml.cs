using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
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

        // Window.Title est une string : le contrôle Localize ne peut pas s'y
        // appliquer, un binding direct affiche le tag brut ("{Connection}").
        DataContextChanged += (_, _) => _ = LocalizeTitleAsync();
    }

    async Task LocalizeTitleAsync()
    {
        if (DataContext is not IMainViewModel vm) return;

        var title = vm.Title;
        if (string.IsNullOrWhiteSpace(title)) return;

        try
        {
            Title = await vm.LocalizationService.LocalizeAsync(title);
        }
        catch
        {
            Title = title;
        }
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
    /// Sans TransparencyLevelHint le niveau effectif est None → fond opaque aux couleurs
    /// du thème (le backdrop Mica/Acrylic rendait la dominante grise au lieu du noir HLab
    /// et cassait les coins arrondis Win11). La logique Mica/Blur est conservée au cas où
    /// une fenêtre redemande un hint : Windows dessine alors son propre backdrop derrière
    /// une fenêtre transparente ; X11 Blur (KWin) exige une teinte translucide par-dessus.
    /// </summary>
    void UpdateBackdrop()
    {
        var level = ActualTransparencyLevel;

        if (level == WindowTransparencyLevel.Mica || level == WindowTransparencyLevel.AcrylicBlur)
        {
            Background = Brushes.Transparent;
            return;
        }

        // Blur : teinte translucide par-dessus le flou du compositeur.
        // Opaque : même clé que le fond de la DefaultWindow WPF
        // (Header.Active.Background, #0c0c16 en sombre — bleu-noir profond).
        if (level == WindowTransparencyLevel.Blur)
        {
            var tint = ThemeBackgroundColor();
            Background = new SolidColorBrush(new Color(0xA8, tint.R, tint.G, tint.B));
            return;
        }

        var color = OpaqueBackgroundColor();
        Background = new SolidColorBrush(new Color(0xFF, color.R, color.G, color.B));
    }

    Color OpaqueBackgroundColor()
    {
        if (this.TryFindResource("HLab.Colors.Header.Active.Background", ActualThemeVariant, out var value)
            && value is Color header)
            return header;

        if (this.TryFindResource("HLab.Colors.Background", ActualThemeVariant, out value)
            && value is Color hlab)
            return hlab;

        return ThemeBackgroundColor();
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

    /// <summary>
    /// La barre de titre système est masquée (ExtendClientAreaToDecorationsHint) :
    /// le déplacement se fait en tirant la bande haute de la fenêtre, comme la
    /// ChromeWindow WPF. Double-clic : bascule maximisé/normal.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        if (e.GetPosition(this).Y > DragStripHeight) return;
        if (IsInteractive(e.Source)) return;

        if (e.ClickCount == 2)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            return;
        }

        BeginMoveDrag(e);
    }

    const double DragStripHeight = 40.0;

    bool IsInteractive(object? source)
    {
        var visual = source as Visual;
        while (visual is not null && !ReferenceEquals(visual, this))
        {
            if (visual is Button or ToggleButton or TextBox or ComboBox or MenuItem or TabItem or ScrollBar or Slider)
                return true;
            visual = visual.GetVisualParent();
        }
        return false;
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
