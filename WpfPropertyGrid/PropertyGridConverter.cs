namespace WpfPropertyGrid;

public class PropertyGridConverter : IValueConverter
{
    private static Type? GetParameterAsType(object? parameter)
    {
        if (parameter == null)
            return null;

        string typeName = string.Format("{0}", parameter);
        if (string.IsNullOrWhiteSpace(typeName))
            return null;

        return TypeResolutionService.ResolveType(typeName);
    }

    public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture) => targetType == null ? value : ConversionService.ChangeType(value, targetType, null, culture);
    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        var parameterType = GetParameterAsType(parameter);
        if (parameterType != null)
        {
            value = ConversionService.ChangeType(value, parameterType, null, culture);
        }

        return targetType == null ? value : ConversionService.ChangeType(value, targetType, null, culture);
    }
}