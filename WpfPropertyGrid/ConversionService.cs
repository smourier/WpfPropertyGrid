namespace WpfPropertyGrid;

public static class ConversionService
{
    public static bool TryConvertType<T>(object? input, out T? value) => TryConvertType(input, null, out value);
    public static bool TryConvertType<T>(object? input, IFormatProvider? provider, out T? value)
    {
        var result = PropertyGridServiceProvider.Current.GetService<IConverter>().TryConvert(input, typeof(T), provider, out object? v);
        if (!result)
        {
            if (v == null)
            {
                if (typeof(T).IsValueType)
                {
                    value = Activator.CreateInstance<T>();
                }
                else
                {
                    value = default;
                }
            }
            else
            {
                value = (T)v;
            }
            return false;
        }

        value = (T?)v;
        return result;
    }

    public static bool TryConvertObjectType(object? input, Type conversionType, out object? value) => TryConvertObjectType(input, conversionType, null, out value);
    public static bool TryConvertObjectType(object? input, Type conversionType, IFormatProvider? provider, out object? value) => PropertyGridServiceProvider.Current.GetService<IConverter>().TryConvert(input, conversionType, provider, out value);
    public static object? ConvertObjectType(object? input, Type conversionType, object? defaultValue = null, IFormatProvider? provider = null)
    {
        ArgumentNullException.ThrowIfNull(conversionType);
        if (defaultValue == null && conversionType.IsValueType)
        {
            defaultValue = Activator.CreateInstance(conversionType);
        }

        if (TryConvertObjectType(input, conversionType, provider, out object? value))
            return value;

        return defaultValue;
    }

    public static T? ConvertType<T>(object? input, T? defaultValue = default, IFormatProvider? provider = null)
    {
        if (TryConvertType(input, provider, out T? value))
            return value;

        return defaultValue;
    }
}
