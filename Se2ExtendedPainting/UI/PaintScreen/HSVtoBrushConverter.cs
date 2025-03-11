using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Keen.VRage.Library.Mathematics;
using Keen.VRage.UI.Shared.Extensions;

namespace Se2ExtendedPainting.UI.PaintScreen;

public class HSVtoBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ColorHSV color) return new SolidColorBrush(color.ToAvalonia());
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}