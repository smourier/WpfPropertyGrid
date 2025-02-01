namespace WpfPropertyGrid;

public interface IActivator
{
    object? CreateInstance(Type type, params object?[]? args);
}
