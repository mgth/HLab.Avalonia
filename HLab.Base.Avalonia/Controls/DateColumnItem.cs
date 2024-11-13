using HLab.Base.Extensions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using HLab.Base.Avalonia.DependencyHelpers;

namespace HLab.Base.Avalonia.Controls;

using H = DependencyHelper<DateColumnItem>;
public class DateColumnItem : TextBlock
{
    public static readonly StyledProperty<DateTime?> DateProperty =
        H.Property<DateTime?>()
            .BindModeDefault(BindingMode.TwoWay)
            .OnChanged((e, a) => e.SetDate(a.NewValue.Value, e.DayValid)).Register();

    public static readonly StyledProperty<bool> DayValidProperty =
        H.Property<bool>()
            .BindModeDefault(BindingMode.TwoWay)
            .OnChanged((e, a) => e.SetDate(e.Date, a.NewValue.Value)).Register();

    public DateTime? Date
    {
        get => GetValue(DateProperty).ToLocalTime();
        set => SetValue(DateProperty, value.ToLocalTime());
    }

    public bool DayValid
    {
        get => GetValue(DayValidProperty);
        set => SetValue(DayValidProperty, value);
    }
    void SetDate(DateTime? date, bool dayValid)
    {
        if (date.HasValue)
        {
            var d = date.Value.ToLocalTime();

            Text = d.ToString(dayValid ? "dd/MM/yyyy" : "MM/yyyy");
        }
        else
        {
            Text = "";
        }
    }
}