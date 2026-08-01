using Avalonia;
using Avalonia.Controls;

namespace HLab.Base.Avalonia.Controls;

/// <summary>
/// Grid qui place automatiquement ses enfants en lignes de N colonnes
/// (portage du PropertyGrid WPF : label / champ sur deux colonnes).
/// </summary>
public class PropertyGrid : Grid
{
    protected override Size MeasureOverride(Size constraint)
    {
        UpdateChildren();

        if (Children.Count > 0)
        {
            var maxRowIndex = 0;
            foreach (var child in Children)
            {
                var r = GetRow(child);
                if (r > maxRowIndex) maxRowIndex = r;
            }

            for (var i = RowDefinitions.Count; i <= maxRowIndex; i++)
                RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }

        return base.MeasureOverride(constraint);
    }

    void UpdateChildren()
    {
        var colSize = ColumnDefinitions.Count;
        if (colSize == 0) return;

        var col = 0;
        var row = 0;
        foreach (var child in Children)
        {
            if (child == null) continue;

            if (GetRow(child) != row)
                SetRow(child, row);

            if (GetColumn(child) != col)
                SetColumn(child, col);

            if (child is not Border)
            {
                col += GetColumnSpan(child);
                if (col >= colSize)
                {
                    col = 0;
                    row++;
                }
            }
        }
    }
}
