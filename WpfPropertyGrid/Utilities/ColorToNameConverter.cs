namespace WpfPropertyGrid.Utilities;

public class ColorToNameConverter : IValueConverter
{
    private readonly static ConcurrentDictionary<Color, string> _colorNames = GetColorNames();

    private static ConcurrentDictionary<Color, string> GetColorNames()
    {
        var dic = new ConcurrentDictionary<Color, string>();
        foreach (var prop in typeof(Colors).GetProperties())
        {
            if (prop.GetValue(null) is Color color)
            {
                dic[color] = prop.Name;
            }
        }
        return dic;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Color color)
        {
            var opaque = color;
            opaque.A = 255;
            if (!_colorNames.TryGetValue(opaque, out var name))
            {
                name = $"#FF{opaque.R:X2}{opaque.G:X2}{opaque.B:X2}";
            }
            else
            {
            }
            return name;
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
