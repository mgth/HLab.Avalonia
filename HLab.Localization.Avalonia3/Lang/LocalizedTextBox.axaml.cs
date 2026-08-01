using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using HLab.Base.Extensions;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Data;
using HLab.Mvvm.Annotations;
using HLab.Base.Avalonia.DependencyHelpers;

namespace HLab.Localization.Avalonia.Lang;

using H = DependencyHelper<LocalizedTextBox>;
/// <summary>
/// Logique d'interaction pour LocalizeTextBox.xaml
/// </summary>
///
public partial class LocalizedTextBox : UserControl
{
    public LocalizedTextBox()
    {
        InitializeComponent();
        // Le service de localisation (propriété attachée héritée) n'est disponible
        // qu'une fois rattaché à l'arbre : relocaliser à ce moment-là.
        AttachedToVisualTree += (_, _) => _ = RefreshAsync();
    }

    async Task RefreshAsync()
    {
        var text = Text;
        var localize = GetValue(Localize.LocalizationServiceProperty);

        if (localize == null || string.IsNullOrEmpty(text))
        {
            TextBoxDisabled.Text = text;
            return;
        }

        TextBoxDisabled.Text = await localize.LocalizeAsync(text).ConfigureAwait(true);
        await PopulateAsync(text);
    }

    void SetReadOnly(bool readOnly)
    {
        if (readOnly)
        {
            TextBoxEnabled.IsVisible = false;
            TextBoxDisabled.IsVisible = true;
            Button.IsVisible = false;
            LocalizationOpened = false;
        }
        else
        {
            TextBoxEnabled.IsVisible = true;
            TextBoxDisabled.IsVisible = true;
            Button.IsVisible = true;
        }
    }

    public static readonly StyledProperty<string> TextProperty =
        H.Property<string>()
            .BindModeDefault(BindingMode.TwoWay)
            .OnChanged(async (e,a) =>
            {
                await e.RefreshAsync();
            })
            .Register();

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        H.Property<bool>()
            .OnChanged((e,a) =>
            {
                e.SetReadOnly(e.IsReadOnly);
            })
            .Register();

    public static readonly StyledProperty<bool> LocalizationOpenedProperty =
        H.Property<bool>()
            .OnChanged((e,a) =>
            {
                e.SetLocalizationOpened(e.LocalizationOpened);
            })
            .Register();

    async void SetLocalizationOpened(bool opened)
    {
        if (opened)
        {
            DataGrid.IsVisible = true;
            await PopulateAsync(Text);
        }
        else
        {
            DataGrid.IsVisible = false;
            UnPopulate();
        }
    }

    //[Content]
    public string Text
    {
        get => (string) GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsReadOnly
    {
        get => (bool) GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public bool LocalizationOpened
    {
        get => (bool) GetValue(LocalizationOpenedProperty);
        set => SetValue(LocalizationOpenedProperty, value);
    }


    public ObservableCollection<ILocalizeEntry> Translations { get; } = new();

    async Task PopulateAsync(string source)
    {
        var service = GetValue(Localize.LocalizationServiceProperty);
        if (service == null || string.IsNullOrEmpty(source)) return;

        var list = source.GetInside('{', '}').ToList();

        Translations.Clear();

        foreach (var s in list)
        {
            var entry = await service.GetLocalizeEntryAsync("fr-fr", s);
            if (entry != null) Translations.Add(entry);
        }
    }

    void UnPopulate()
    {
        Translations.Clear();
    }

    void Button_OnClick(object sender, RoutedEventArgs e)
    {
        LocalizationOpened = !LocalizationOpened;
    }
}