namespace WpfPropertyGrid;

public class BaseActivator : IActivator
{
    public virtual object? CreateInstance(Type type, params object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (type == typeof(Utilities.DynamicObject))
            return new Utilities.DynamicObject();

        if (type == typeof(PropertyGridProperty))
        {
            ArgumentNullException.ThrowIfNull(args);
            if (args.Length == 0)
                throw new ArgumentException(null, nameof(args));

            if (args[0] is not PropertyGridDataProvider provider)
                throw new ArgumentException(null, nameof(args));

            return new PropertyGridProperty(provider);
        }

        return Activator.CreateInstance(type, args);
    }

    public T? CreateInstance<T>(params object?[]? args)
    {
        if (typeof(T) == typeof(Utilities.DynamicObject))
            return (T)(object)new Utilities.DynamicObject();

        if (typeof(T) == typeof(PropertyGridProperty))
        {
            ArgumentNullException.ThrowIfNull(args);
            if (args.Length == 0)
                throw new ArgumentException(null, nameof(args));

            if (args[0] is not PropertyGridDataProvider provider)
                throw new ArgumentException(null, nameof(args));

            return (T)(object)new PropertyGridProperty(provider);
        }

        return (T?)Activator.CreateInstance(typeof(T), args);
    }
}
