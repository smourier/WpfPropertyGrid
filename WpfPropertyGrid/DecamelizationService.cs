namespace WpfPropertyGrid;

public static class DecamelizationService
{
    public static string? Decamelize(string? text, DecamelizeOptions? options = null) => PropertyGridServiceProvider.Current.GetService<IDecamelizer>().Decamelize(text, options);
}
