namespace WpfPropertyGrid;

public class ChangeTypeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => ConversionService.ConvertObjectType(value, targetType, null, culture);
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value;
}
