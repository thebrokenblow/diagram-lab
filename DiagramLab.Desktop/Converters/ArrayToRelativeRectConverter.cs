using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace DiagramLab.Desktop.Converters;

public class ArrayToRelativeRectConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double[] { Length: 4 } array)
        {
            return new RelativeRect(array[0], array[1], array[2], array[3], RelativeUnit.Absolute);
        }
        
        throw new ArgumentException($"Expected double[4], but got {value?.GetType().Name ?? "null"}");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("ConvertBack is not supported for ArrayToRelativeRectConverter");
    }
}