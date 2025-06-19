using Avalonia.Data.Converters;
using Avalonia.Media;

namespace HLab.Base.Avalonia.Converters;

public class ToStringValueConverter : ToValueConverter<string> { }
public class ToBrushValueConverter : ToValueConverter<Brush> { }
public class ToObjectValueConverter : ToValueConverter<object> { }
public class ToBooleanValueConverter : ToValueConverter<bool> { }

public class ToValueConverter<T> : IValueConverter
{
    public T? NullValue { get; set; }
    public T? FalseValue { get; set; }
    public T? TrueValue { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) =>
       value switch
       {
          bool b => b ? TrueValue : FalseValue,
          string s => string.IsNullOrWhiteSpace(s) ? FalseValue : TrueValue,
          int i => i == 0 ? FalseValue : TrueValue,
          _ => NullValue
       };

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
       if (value is not T typedValue) return null;
       if (Equals(typedValue, TrueValue)) return true;
       if (Equals(typedValue, FalseValue)) return false;

       return null;
    }
}