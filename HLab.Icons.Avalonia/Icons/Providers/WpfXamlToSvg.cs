using System.Text;
using System.Xml.Linq;

namespace HLab.Icons.Avalonia.Icons.Providers;

/// <summary>
/// Convertit les icônes XAML WPF stockées en base (sortie de l'outil svg2xaml :
/// Viewbox &gt; Canvas imbriqués (ScaleTransform/TranslateTransform) &gt; Path avec
/// PathGeometry) en source SVG, pour les rendre via le pipeline Svg.Skia — le
/// chargeur XAML Avalonia ne comprend pas le markup WPF. Couvre le sous-ensemble
/// que génère l'outil ; retourne null si la structure ne s'y prête pas.
/// </summary>
public static class WpfXamlToSvg
{
    public static string? TryConvert(string xaml)
    {
        try
        {
            var root = XDocument.Parse(xaml).Root;
            if (root is null) return null;

            // La racine utile est le premier Canvas dimensionné.
            var canvas = root.Name.LocalName == "Canvas"
                ? root
                : root.Descendants().FirstOrDefault(e => e.Name.LocalName == "Canvas" && e.Attribute("Width") is not null);
            if (canvas is null) return null;

            var w = (string?)canvas.Attribute("Width") ?? "100";
            var h = (string?)canvas.Attribute("Height") ?? "100";

            var sb = new StringBuilder();
            sb.Append($"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {w} {h}\">");
            AppendChildren(sb, canvas);
            sb.Append("</svg>");

            // Aucun Path trouvé : ne pas retourner un SVG vide.
            return sb.ToString().Contains("<path") ? sb.ToString() : null;
        }
        catch
        {
            return null;
        }
    }

    static void AppendChildren(StringBuilder sb, XElement parent)
    {
        foreach (var e in parent.Elements())
        {
            switch (e.Name.LocalName)
            {
                case "Canvas":
                    var t = CanvasTransform(e);
                    sb.Append(t.Length > 0 ? $"<g transform=\"{t}\">" : "<g>");
                    AppendChildren(sb, e);
                    sb.Append("</g>");
                    break;

                case "Path":
                    AppendPath(sb, e);
                    break;
            }
        }
    }

    static string CanvasTransform(XElement canvas)
    {
        var parts = new List<string>();

        var left = (string?)canvas.Attribute("Canvas.Left");
        var top = (string?)canvas.Attribute("Canvas.Top");
        if (left is not null || top is not null)
            parts.Add($"translate({left ?? "0"},{top ?? "0"})");

        var render = canvas.Elements().FirstOrDefault(x => x.Name.LocalName == "Canvas.RenderTransform");
        if (render is not null)
        {
            var transforms = render.Descendants()
                .Where(x => x.Name.LocalName is "ScaleTransform" or "TranslateTransform" or "MatrixTransform")
                .ToList();

            // WPF applique les enfants d'un TransformGroup dans l'ordre ;
            // une liste de transforms SVG s'applique de droite à gauche.
            transforms.Reverse();

            foreach (var tr in transforms)
            {
                switch (tr.Name.LocalName)
                {
                    case "ScaleTransform":
                        parts.Add($"scale({(string?)tr.Attribute("ScaleX") ?? "1"},{(string?)tr.Attribute("ScaleY") ?? "1"})");
                        break;
                    case "TranslateTransform":
                        parts.Add($"translate({(string?)tr.Attribute("X") ?? "0"},{(string?)tr.Attribute("Y") ?? "0"})");
                        break;
                    case "MatrixTransform":
                        var m = ((string?)tr.Attribute("Matrix"))?.Replace(",", " ");
                        if (m is not null) parts.Add($"matrix({m})");
                        break;
                }
            }
        }

        return string.Join(" ", parts);
    }

    static void AppendPath(StringBuilder sb, XElement path)
    {
        var geometry = path.Elements().FirstOrDefault(x => x.Name.LocalName == "Path.Data")
            ?.Elements().FirstOrDefault(x => x.Name.LocalName == "PathGeometry");

        var data = (string?)geometry?.Attribute("Figures") ?? (string?)path.Attribute("Data");
        if (string.IsNullOrWhiteSpace(data)) return;

        // La mini-syntaxe des figures WPF est celle de SVG, au préfixe F0/F1 près.
        data = data.TrimStart();
        var fillRule = (string?)geometry?.Attribute("FillRule");
        if (data.StartsWith("F0", StringComparison.OrdinalIgnoreCase)) { fillRule = "EvenOdd"; data = data[2..]; }
        else if (data.StartsWith("F1", StringComparison.OrdinalIgnoreCase)) { fillRule = "Nonzero"; data = data[2..]; }

        sb.Append("<path");

        var fill = (string?)path.Attribute("Fill");
        sb.Append($" fill=\"{(fill is null ? "none" : fill.ToLowerInvariant())}\"");

        // Défaut WPF = EvenOdd, défaut SVG = nonzero : expliciter.
        sb.Append($" fill-rule=\"{(fillRule?.Equals("Nonzero", StringComparison.OrdinalIgnoreCase) is true ? "nonzero" : "evenodd")}\"");

        var stroke = (string?)path.Attribute("Stroke");
        if (stroke is not null) sb.Append($" stroke=\"{stroke.ToLowerInvariant()}\"");

        var thickness = (string?)path.Attribute("StrokeThickness");
        if (thickness is not null) sb.Append($" stroke-width=\"{thickness}\"");

        var opacity = (string?)path.Attribute("Opacity");
        if (opacity is not null) sb.Append($" opacity=\"{opacity}\"");

        sb.Append($" d=\"{data}\"/>");
    }
}
