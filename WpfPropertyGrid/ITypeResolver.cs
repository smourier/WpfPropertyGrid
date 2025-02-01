namespace WpfPropertyGrid;

public interface ITypeResolver
{
    Type? ResolveType(string fullName, bool throwOnError);
}
