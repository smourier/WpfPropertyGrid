namespace WpfPropertyGrid;

public class UniversalConverter : IValueConverter
{
    public virtual object? DefaultValue { get; set; }
    public virtual ObservableCollection<UniversalConverterCase> Switch { get; } = [];

    public virtual object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => ConversionService.ChangeType(parameter, targetType, null, culture);
    public virtual object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        foreach (var @case in Switch)
        {
            if (@case.Matches(value, parameter, culture))
            {
                if ((@case.Options & UniversalConverterOptions.ConvertedValueIsConverterParameter) == UniversalConverterOptions.ConvertedValueIsConverterParameter)
                    return ConversionService.ChangeType(parameter, targetType, culture);

                return ConversionService.ChangeType(@case.ConvertedValue, targetType, null, culture);
            }
        }
        return ConversionService.ChangeType(DefaultValue, targetType, null, culture);
    }
}
