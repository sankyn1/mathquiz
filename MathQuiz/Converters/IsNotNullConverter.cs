﻿// File: Converters/IsNotNullConverter.cs
using Microsoft.Maui.Controls;
using System.Globalization;

namespace MathQuiz.Converters;

public class IsNotNullConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value != null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
