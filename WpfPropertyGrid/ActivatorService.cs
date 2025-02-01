namespace WpfPropertyGrid;

public static class ActivatorService
{
    public static object CreateInstance(Type type, params object[] args)
    {
        ArgumentNullException.ThrowIfNull(type);

        object obj = ServiceProvider.Current.GetService<IActivator>().CreateInstance(type, args);
        return obj;
    }

    public static T CreateInstance<T>(params object[] args) => (T)CreateInstance(typeof(T), args);

    public static object CreateInstance(Type type) => CreateInstance(type, null);

    public static T CreateInstance<T>() => (T)CreateInstance(typeof(T), null);
}
