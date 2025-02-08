namespace WpfPropertyGrid;

public static class ActivatorService
{
    public static object? CreateInstance(Type type, params object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(type);
        return PropertyGridServiceProvider.Current.GetService<IActivator>().CreateInstance(type, args);
    }

    public static T? CreateInstance<T>(params object?[]? args) => PropertyGridServiceProvider.Current.GetService<IActivator>().CreateInstance<T>(args);
}
