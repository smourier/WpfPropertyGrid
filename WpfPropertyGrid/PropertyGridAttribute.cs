namespace WpfPropertyGrid;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class PropertyGridAttribute : Attribute
{
    public object? Value { get; set; }
    public string? Name { get; set; }
    public Type? Type { get; set; } = typeof(object);

    public override object TypeId => Name ?? string.Empty;
}