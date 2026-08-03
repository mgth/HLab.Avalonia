using System.Text.RegularExpressions;
using Avalonia.Media;

namespace HLab.Icons.Avalonia.Icons.Providers;

internal static class SvgForeground
{
    // Le noir sous toutes ses écritures : #000000, #FF000000 (ARGB des conversions
    // XAML), #000 (raccourci), black/Black (couleur nommée) — en valeur d'attribut
    // ("...") comme en style (:...;). Les bornes interdisent de mordre un préfixe
    // d'une autre couleur (#000080, #FF0000...).
    static readonly Regex Black = new(
        "(?<=[\"':\\s])(?:#FF000000|#000000|#000|black)(?=[\"';\\s/>])",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // fill= présent dans la balise racine (sans matcher fill-rule=).
    static readonly Regex RootFill = new(@"\bfill\s*=", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Applique le foreground à un source SVG : remplace les noirs explicites et
    /// pose fill sur la racine &lt;svg&gt; quand elle n'en a pas — le défaut SVG est
    /// noir, les chemins sans attribut fill restaient donc noirs quel que soit le
    /// foreground (invisibles en thème sombre). Les autres couleurs explicites
    /// (drapeaux, icônes colorées) ne sont pas touchées.
    /// </summary>
    public static string Apply(string source, uint foregroundColor)
    {
        var color = Color.FromUInt32(foregroundColor);
        var fg = $"#{color.R:X2}{color.G:X2}{color.B:X2}";

        var src = Black.Replace(source, fg);

        var i = src.IndexOf("<svg", StringComparison.OrdinalIgnoreCase);
        if (i >= 0)
        {
            var end = src.IndexOf('>', i);
            if (end > i && !RootFill.IsMatch(src.Substring(i, end - i)))
                src = src.Insert(i + 4, $" fill=\"{fg}\"");
        }

        return src;
    }
}
