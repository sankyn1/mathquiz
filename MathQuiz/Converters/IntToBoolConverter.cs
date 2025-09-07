// File: Converters/IntToBoolConverter.cs
using Microsoft.Maui.Controls;
using System.Globalization;

namespace MathQuiz.Converters;

public class IntToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intValue && parameter is string paramStr && int.TryParse(paramStr, out var compareValue))
        {
            return intValue == compareValue;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
