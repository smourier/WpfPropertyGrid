namespace WpfPropertyGrid;

public class UniversalConverter : IValueConverter
{
    public virtual object? DefaultValue { get; set; }
    public virtual ObservableCollection<UniversalConverterCase> Switch { get; } = [];

    public virtual object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => ConversionService.ConvertObjectType(parameter, targetType, null, culture);
    public virtual object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        foreach (var @case in Switch)
        {
            if (@case.Matches(value, parameter, culture))
            {
                if (@case.Options.HasFlag(UniversalConverterOptions.ConvertedValueIsConverterParameter))
                    return ConversionService.ConvertObjectType(parameter, targetType, culture);

                if (@case.Options.HasFlag(UniversalConverterOptions.ConvertedValueIsConverterValue))
                    return ConversionService.ConvertObjectType(value, targetType, culture);

                return ConversionService.ConvertObjectType(@case.ConvertedValue, targetType, null, culture);
            }
        }
        return ConversionService.ConvertObjectType(DefaultValue, targetType, null, culture);
    }
}
