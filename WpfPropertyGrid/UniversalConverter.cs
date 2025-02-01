namespace WpfPropertyGrid;

public class UniversalConverter : IValueConverter
{
    private readonly ObservableCollection<UniversalConverterCase> _cases = [];

    public virtual object? DefaultValue { get; set; }
    public virtual ObservableCollection<UniversalConverterCase> Switch => _cases;

    public virtual object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (_cases.Count == 0)
            return ConversionService.ChangeType(value, targetType, culture);

        foreach (var c in _cases)
        {
            if (c.Matches(value, parameter, culture))
            {
                if ((c.Options & UniversalConverterOptions.ConvertedValueIsConverterParameter) == UniversalConverterOptions.ConvertedValueIsConverterParameter)
                    return ConversionService.ChangeType(parameter, targetType, culture);

                return ConversionService.ChangeType(c.ConvertedValue, targetType, null, culture);
            }
        }

        return ConversionService.ChangeType(DefaultValue, targetType, null, culture);
    }

    public virtual object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => ConversionService.ChangeType(parameter, targetType, null, culture);
}
