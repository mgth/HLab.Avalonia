using Avalonia;

namespace HLab.Base.Avalonia.Controls;

/// <summary>
/// Contrôle avec état « champ obligatoire non rempli » : MandatoryProperty désigne
/// la propriété dont le binding identifie le champ (parité HLab.Base.Wpf).
/// </summary>
public interface IMandatoryNotFilled
{
    AvaloniaProperty MandatoryProperty { get; }
    bool MandatoryNotFilled { get; set; }
}
