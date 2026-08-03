using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Svg.Skia;
using Avalonia.Threading;
using HLab.Mvvm.Annotations;
using Svg.Model.Services;

namespace HLab.Icons.Avalonia.Icons.Providers;

// Rendu direct via Svg.Skia (comme IconProviderSvgFromUri) : l'ancienne voie
// XSL svg2xaml n'est pas disponible côté Avalonia et ne couvrait qu'un
// sous-ensemble du SVG.
public class IconProviderSvgFromSource(string source, string name, int? foreColor) : IIconProvider
{
    public object? Get(uint foregroundColor = 0)
    {
        try
        {
            var img = BuildImageSource(foregroundColor);
            return Dispatcher.UIThread.CheckAccess()
                ? new Image { Source = img }
                : Dispatcher.UIThread.InvokeAsync(() => new Image { Source = img }).GetTask().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Icons] échec SVG source '{name}': {ex.GetType().Name} {ex.Message}");
            return null;
        }
    }

    public async Task<object?> GetAsync(uint foregroundColor = 0)
    {
        try
        {
            var img = BuildImageSource(foregroundColor);
            return await Dispatcher.UIThread.InvokeAsync(() => new Image { Source = img });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Icons] échec SVG source '{name}': {ex.GetType().Name} {ex.Message}");
            return null;
        }
    }

    SvgImage BuildImageSource(uint foregroundColor)
    {
        var src = SvgForeground.Apply(source, foregroundColor);

        var doc = SvgService.FromSvg(src);
        var svg = SvgSource.LoadFromSvgDocument(doc);
        return new SvgImage { Source = svg };
    }

    public Task<string> GetTemplateAsync(uint foreground = 0)
        => Task.FromResult("<ContentControl/>");
}
