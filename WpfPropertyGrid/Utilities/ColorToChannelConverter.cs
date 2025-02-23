namespace WpfPropertyGrid.Utilities;

public class ColorToChannelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Color color && parameter is string channel)
        {
            switch (channel.ToLowerInvariant())
            {
                case "a":
                    return color.A;

                case "r":
                    return color.R;

                case "g":
                    return color.G;

                case "b":
                    return color.B;

                case "sca":
                    return color.ScA;

                case "scr":
                    return color.ScR;

                case "scg":
                    return color.ScG;

                case "scb":
                    return color.ScB;
            }
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
