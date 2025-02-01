namespace WpfPropertyGrid;

public class BaseTypeResolver : ITypeResolver
{
    public virtual Type? ResolveType(string fullName, bool throwOnError)
    {
        ArgumentNullException.ThrowIfNull(fullName);
        return Type.GetType(fullName, throwOnError);
    }
}
