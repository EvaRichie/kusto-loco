using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace lokqlDx.Wpf.Views.Converters;

public sealed class FontFamilyNameConverter : IValueConverter
{
    public const string DefaultFontFamilyName = "Consolas";

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (targetType == typeof(FontFamily))
        {
            if (value is string fontFamilyName)
            {
                var fontFamilyInstance = new FontFamily(fontFamilyName);
                return fontFamilyInstance;
            }

            return new FontFamily(DefaultFontFamilyName);
        }

        return default;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is FontFamily fontFamily)
        {
            return fontFamily.FamilyNames.FirstOrDefault().Value;
        }

        return default;
    }
}
