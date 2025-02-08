namespace WpfPropertyGrid;

public static class TypeResolutionService
{
    public static Type? ResolveType(string fullName, bool throwOnError = false) => PropertyGridServiceProvider.Current.GetService<ITypeResolver>().ResolveType(fullName, throwOnError);
}
