// File: Converters/StringIsNullOrEmptyToBoolConverter.cs
using Microsoft.Maui.Controls;
using System.Globalization;

namespace MathQuiz.Converters;

public class StringIsNullOrEmptyToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return !string.IsNullOrEmpty(value?.ToString());
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
