namespace WpfPropertyGrid;

public static class DecamelizationService
{
    public static string? Decamelize(string? text, DecamelizeOptions? options = null) => ServiceProvider.Current.GetService<IDecamelizer>().Decamelize(text, options);
}
