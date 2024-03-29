﻿/*
  HLab.Mvvm
  Copyright (c) 2021 Mathieu GRENET.  All right reserved.

  This file is part of HLab.Mvvm.

    HLab.Mvvm is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    HLab.Mvvm is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MouseControl.  If not, see <http://www.gnu.org/licenses/>.

	  mailto:mathieu@mgth.fr
	  http://www.mgth.fr
*/

using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace HLab.Mvvm.Avalonia.Converters;

public class MultiScaleConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        var vs = values.OfType<double>().ToList();

        if (!vs.Any()) return 0.1;

        var v = vs.Min();

        if (parameter is not string s) return null;
        var p = s.Split('|');

        var scale = double.Parse(p[0], CultureInfo.InvariantCulture);
        var result = v * scale;

        if (double.IsNaN(result) || double.IsInfinity(result)) result = 0.1;

        var min = (p.Length > 1)?double.Parse(p[1], CultureInfo.InvariantCulture):0.1;
        var max = (p.Length > 2)?double.Parse(p[2], CultureInfo.InvariantCulture):35791;

        result = Math.Min(Math.Max(result, min), max);

        if(targetType == typeof(CornerRadius))
            return new CornerRadius(result);

        if(targetType == typeof(double))
            return result;

        return null;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("MultiScaleConverter : ConvertBack");
    }

}