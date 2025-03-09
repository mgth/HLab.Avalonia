using Avalonia;
using Avalonia.Controls;
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
