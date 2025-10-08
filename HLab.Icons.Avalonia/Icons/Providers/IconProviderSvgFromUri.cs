using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Svg.Skia;
using Avalonia.Threading;
using HLab.Mvvm.Annotations;
using Svg.Model;
using Avalonia.Svg.Skia;
using System.Text;
using Svg.Model.Services;

namespace HLab.Icons.Avalonia.Icons.Providers;


public class IconProviderSvgFromUri(Uri uri, Color? foreColor) : IIconProvider
{
   string _source = "";

   public object? Get(uint foregroundColor = 0)
   {
      return GetAsync(foregroundColor).GetAwaiter().GetResult();
   }
   
   public async Task<object?> GetAsync(uint foregroundColor = 0)
   {
      var color = Color.FromUInt32(foregroundColor);

      var foregroundString = $"{color.R:X2}{color.G:X2}{color.B:X2}";

      var stream = AssetLoader.Open(uri);
      var reader = new StreamReader(stream);

      _source = await reader.ReadToEndAsync();

      _source = _source.Replace("\"#000000\"", $"\"#{foregroundString}\"");

      _source = _source.Replace(":#000000", $":#{foregroundString}");
      
      var doc = SvgService.FromSvg(_source);
      var src = SvgSource.LoadFromSvgDocument(doc);
      var img = new SvgImage { Source = src };

      return await Dispatcher.UIThread.InvokeAsync(() => 
      {
         var icon = new Image {Source = img };
         // Retourner un TextBlock temporaire en attendant la résolution du namespace SVG
         return icon;
      });
   }

   public Task<string> GetTemplateAsync(uint foreground = 0)
   {
      return Task.FromResult($"<Svg Path=\"{uri.AbsoluteUri}\"/>");
   }
}