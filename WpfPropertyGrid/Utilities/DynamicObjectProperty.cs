namespace WpfPropertyGrid.Utilities;

public class DynamicObjectProperty : PropertyDescriptor
{
    private Type _type;
    private bool _isReadOnly;
    private object? _defaultValue;

    public DynamicObjectProperty(PropertyDescriptor descriptor)
        : base(descriptor)
    {
        var atts = new List<Attribute>();
        foreach (Attribute att in descriptor.Attributes)
        {
            atts.Add(att);
        }
        Construct(descriptor.Name, descriptor.PropertyType, atts);
    }

    public DynamicObjectProperty(string name, Type type, IEnumerable<Attribute>? attributes)
        : base(name, GetAttributes(attributes))
    {
        Construct(name, type, attributes);
    }

    public virtual object? DefaultValue { get => _defaultValue; set { _defaultValue = ConversionService.ConvertObjectType(value, _type); } }
    public virtual int SortOrder { get; set; }
    public virtual bool HasDefaultValue { get; set; }
    public override Type ComponentType => typeof(DynamicObject);
    public override bool IsReadOnly => _isReadOnly;
    public override Type PropertyType => _type;

    public override string ToString() => Name + " (" + _type.FullName + ")";
    public override bool ShouldSerializeValue(object component) => false;
    public override bool CanResetValue(object component) => HasDefaultValue;
    public override object? GetValue(object? component)
    {
        if (component is DynamicObject obj)
            return obj.GetPropertyValue(Name, _defaultValue);

        throw new ArgumentException("Component is not of the DynamicObject type", nameof(component));
    }

    [MemberNotNull(nameof(_type))]
    protected virtual void Construct(string name, Type type, IEnumerable<Attribute>? attributes)
    {
        _type = type;

        var ro = attributes?.OfType<ReadOnlyAttribute>().FirstOrDefault();
        if (ro != null)
        {
            _isReadOnly = ro.IsReadOnly;
        }

        var dv = attributes?.OfType<DefaultValueAttribute>().FirstOrDefault();
        if (dv != null)
        {
            HasDefaultValue = true;
            _defaultValue = ConversionService.ConvertObjectType(dv.Value, _type);
        }
        else
        {
            _defaultValue = ConversionService.ConvertObjectType(null, _type);
        }
    }

    public override void ResetValue(object component)
    {
        if (HasDefaultValue)
        {
            SetValue(component, _defaultValue);
        }
    }

    public override void SetValue(object? component, object? value)
    {
        if (component is DynamicObject obj)
        {
            obj.SetPropertyValue(Name, value);
            return;
        }

        throw new ArgumentException("Component is not of the DynamicObject type", nameof(component));
    }

    private static Attribute[] GetAttributes(IEnumerable<Attribute>? attributes)
    {
        var list = attributes == null ? [] : new List<Attribute>(attributes);
        return [.. list];
    }
}