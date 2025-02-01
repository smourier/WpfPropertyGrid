namespace WpfPropertyGrid;

public static class ConversionService
{
    public static bool TryChangeType<T>(object input, IFormatProvider provider, out T value)
    {
        var b = ServiceProvider.Current.GetService<IConverter>().TryChangeType(input, typeof(T), provider, out object? v);
        if (!b)
        {
            if (v == null)
            {
                if (typeof(T).IsValueType)
                {
                    value = (T)Activator.CreateInstance(typeof(T));
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
        value = (T)v;
        return b;
    }

    public static bool TryChangeType<T>(object? input, out T? value) => TryChangeType(input, null, out value);

    public static bool TryChangeType(object? input, Type conversionType, out object? value) => TryChangeType(input, conversionType, null, out value);

    public static bool TryChangeType(object? input, Type conversionType, IFormatProvider? provider, out object value) => ServiceProvider.Current.GetService<IConverter>().TryChangeType(input, conversionType, provider, out value);

    public static object? ChangeType(object? input, Type conversionType) => ChangeType(input, conversionType, null, null);

    public static object? ChangeType(object? input, Type conversionType, object? defaultValue) => ChangeType(input, conversionType, defaultValue, null);

    public static object? ChangeType(object? input, Type conversionType, object? defaultValue, IFormatProvider provider)
    {
        ArgumentNullException.ThrowIfNull(conversionType);
        if (defaultValue == null && conversionType.IsValueType)
        {
            defaultValue = Activator.CreateInstance(conversionType);
        }

        if (TryChangeType(input, conversionType, provider, out object value))
            return value;

        return defaultValue;
    }

    public static T ChangeType<T>(object? input) => ChangeType(input, default(T));

    public static T? ChangeType<T>(object? input, T? defaultValue) => ChangeType(input, defaultValue, null);

    public static T? ChangeType<T>(object? input, T? defaultValue, IFormatProvider? provider)
    {
        if (TryChangeType(input, provider, out T value))
            return value;

        return defaultValue;
    }
}
