using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace lokqlDx.Wpf.Views.Converters;

public class BoolToValueConverter : IValueConverter
{
    public required object TrueValue { get; set; }

    public object? FalseValue { get; set; }

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue == true ? TrueValue : FalseValue;
        }

        return default;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return default;
    }
}
