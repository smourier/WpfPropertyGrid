namespace WpfPropertyGrid;

public interface IDecamelizer
{
    string? Decamelize(string? text, DecamelizeOptions? options = null);
}
