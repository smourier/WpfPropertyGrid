namespace WpfPropertyGrid;

public class ServiceProvider : IServiceProvider
{
    private readonly ConcurrentDictionary<Type, object?> _services = new();
    private static readonly ServiceProvider _current = new();

    public ServiceProvider()
    {
        ResetDefaultServices();
    }

    protected virtual void ResetDefaultServices()
    {
        if (!_services.ContainsKey(typeof(IConverter)))
        {
            _services[typeof(IConverter)] = new BaseConverter();
        }

        if (!_services.ContainsKey(typeof(IDecamelizer)))
        {
            _services[typeof(IDecamelizer)] = new BaseDecamelizer();
        }

        if (!_services.ContainsKey(typeof(ITypeResolver)))
        {
            _services[typeof(ITypeResolver)] = new BaseTypeResolver();
        }

        if (!_services.ContainsKey(typeof(IActivator)))
        {
            _services[typeof(IActivator)] = new BaseActivator();
        }
    }

    public static ServiceProvider Current => _current;

    public T GetService<T>() => (T)GetService(typeof(T))!;

    public virtual object? SetService(Type serviceType, object? service)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        _services.TryGetValue(serviceType, out var previous);
        _services[serviceType] = service;
        ResetDefaultServices();
        return previous;
    }

    public virtual object? GetService(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        _services.TryGetValue(serviceType, out var value);
        return value;
    }
}
