namespace WpfPropertyGrid;

[ContentProperty(nameof(DataTemplate))]
public class PropertyGridDataTemplate
{
    private class NullableEnum { }
    public static readonly Type NullableEnumType = typeof(NullableEnum);

    private readonly Lazy<List<Type>> _resolvedPropertyTypes;
    private readonly Lazy<List<Type>> _resolvedCollectionItemPropertyTypes;

    public PropertyGridDataTemplate()
    {
        _resolvedPropertyTypes = new(GetResolvedPropertyTypes);
        _resolvedCollectionItemPropertyTypes = new(GetResolvedCollectionItemPropertyTypes);
    }

    public string? PropertyType { get; set; }
    public string? CollectionItemPropertyType { get; set; }
    public DataTemplate? DataTemplate { get; set; }
    public bool? IsCollection { get; set; }
    public bool? IsReadOnly { get; set; }
    public bool? IsError { get; set; }
    public bool? IsValid { get; set; }
    public bool? IsFlagsEnum { get; set; }
    public bool? IsCollectionItemValueType { get; set; }
    public bool? IsValueType { get; set; }
    public string? Category { get; set; }
    public string? Name { get; set; }
    public virtual IReadOnlyList<Type> ResolvedPropertyTypes => _resolvedPropertyTypes.Value;
    public virtual IReadOnlyList<Type> ResolvedCollectionItemPropertyTypes => _resolvedCollectionItemPropertyTypes.Value;

    private List<Type> GetResolvedPropertyTypes()
    {
        var types = new List<Type>();
        var names = PropertyType?.SplitToList<string>('|') ?? [];
        foreach (var name in names)
        {
            if (string.IsNullOrWhiteSpace(name))
                continue;

            Type? type;
            if (name == "System.Nullable`1[System.Enum]")
            {
                type = NullableEnumType;
            }
            else
            {
                type = TypeResolutionService.ResolveType(name);
            }

            if (type != null)
            {
                types.Add(type);
            }
        }
        return types;
    }

    private List<Type> GetResolvedCollectionItemPropertyTypes()
    {
        var types = new List<Type>();
        var names = CollectionItemPropertyType?.SplitToList<string>('|') ?? [];
        foreach (var name in names)
        {
            if (string.IsNullOrWhiteSpace(name))
                continue;

            var type = TypeResolutionService.ResolveType(name);
            if (type != null)
            {
                types.Add(type);
            }
        }
        return types;
    }
}