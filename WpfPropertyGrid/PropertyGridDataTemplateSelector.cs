namespace WpfPropertyGrid;

[ContentProperty("DataTemplates")]
public class PropertyGridDataTemplateSelector : DataTemplateSelector
{
    private PropertyGrid? _propertyGrid;

    public PropertyGridDataTemplateSelector()
    {
        DataTemplates = [];
    }

    public PropertyGrid PropertyGrid => _propertyGrid;

    protected virtual bool Filter(PropertyGridDataTemplate template, PropertyGridProperty property)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(property);

        // check various filters
        if (template.IsCollection.HasValue && template.IsCollection.Value != property.IsCollection)
            return true;

        if (template.IsCollectionItemValueType.HasValue && template.IsCollectionItemValueType.Value != property.IsCollectionItemValueType)
            return true;

        if (template.IsValueType.HasValue && template.IsValueType.Value != property.IsValueType)
            return true;

        if (template.IsReadOnly.HasValue && template.IsReadOnly.Value != property.IsReadOnly)
            return true;

        if (template.IsError.HasValue && template.IsError.Value != property.IsError)
            return true;

        if (template.IsValid.HasValue && template.IsValid.Value != property.IsValid)
            return true;

        if (template.IsFlagsEnum.HasValue && template.IsFlagsEnum.Value != property.IsFlagsEnum)
            return true;

        if (template.Category != null && !property.Category.EqualsIgnoreCase(template.Category))
            return true;

        if (template.Name != null && !property.Name.EqualsIgnoreCase(template.Name))
            return true;

        return false;
    }

    public virtual bool IsAssignableFrom(Type type, Type propertyType, PropertyGridDataTemplate template, PropertyGridProperty property)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(propertyType);
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(property);

        if (type.IsAssignableFrom(propertyType))
        {
            // bool? is assignable from bool, but we don't want that match
            if (!type.IsNullable() || propertyType.IsNullable())
                return true;
        }

        // hack for nullable enums...
        if (type == PropertyGridDataTemplate.NullableEnumType)
        {
            PropertyGridProperty.IsEnumOrNullableEnum(propertyType, out _, out var nullable);
            if (nullable)
                return true;
        }

        var options = PropertyGridOptionsAttribute.FromProperty(property);
        if (options != null)
        {
            if ((type.IsEnum || type == typeof(Enum)) && options.IsEnum)
            {
                if (!options.IsFlagsEnum)
                    return true;

                if (Extensions.IsFlagsEnum(type))
                    return true;

                if (template.IsFlagsEnum.HasValue && template.IsFlagsEnum.Value)
                    return true;
            }
        }

        return false;
    }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        ArgumentNullException.ThrowIfNull(container);

        if (item is not PropertyGridProperty property)
            return base.SelectTemplate(item, container);

        var propTemplate = PropertyGridOptionsAttribute.SelectTemplate(property, item, container);
        if (propTemplate != null)
            return propTemplate;

        _propertyGrid ??= container.GetVisualSelfOrParent<PropertyGrid>();

        if (_propertyGrid != null && _propertyGrid.ValueEditorTemplateSelector != null && _propertyGrid.ValueEditorTemplateSelector != this)
        {
            DataTemplate template = _propertyGrid.ValueEditorTemplateSelector.SelectTemplate(item, container);
            if (template != null)
                return template;
        }

        foreach (PropertyGridDataTemplate template in DataTemplates)
        {
            if (Filter(template, property))
                continue;

            if (template.IsCollection.HasValue && template.IsCollection.Value)
            {
                if (string.IsNullOrWhiteSpace(template.CollectionItemPropertyType) && template.DataTemplate != null)
                    return template.DataTemplate;

                if (property.CollectionItemPropertyType != null)
                {
                    foreach (var type in template.ResolvedCollectionItemPropertyTypes)
                    {
                        if (IsAssignableFrom(type, property.CollectionItemPropertyType, template, property))
                            return template.DataTemplate;
                    }
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(template.PropertyType) && template.DataTemplate != null)
                    return template.DataTemplate;

                foreach (var type in template.ResolvedPropertyTypes)
                {
                    if (property.PropertyType != null && IsAssignableFrom(type, property.PropertyType, template, property))
                        return template.DataTemplate;
                }
            }
        }
        return base.SelectTemplate(item, container);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ObservableCollection<PropertyGridDataTemplate> DataTemplates { get; private set; }
}
