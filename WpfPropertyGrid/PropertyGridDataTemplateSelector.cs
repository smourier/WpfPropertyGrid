namespace WpfPropertyGrid;

[ContentProperty("DataTemplates")]
public class PropertyGridDataTemplateSelector : DataTemplateSelector
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ObservableCollection<PropertyGridDataTemplate> DataTemplates { get; } = [];
    public PropertyGrid? PropertyGrid { get; private set; }

    protected virtual bool Filter(PropertyGridDataTemplate template, PropertyGridProperty property)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(property);

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

        if (template.IsValid.HasValue && template.IsValid.Value != property.HasNoError)
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
            if (!type.IsNullable() || propertyType.IsNullable())
                return true;
        }

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

        PropertyGrid ??= container.GetVisualSelfOrParent<PropertyGrid>();
        if (PropertyGrid != null && PropertyGrid.ValueEditorTemplateSelector != null && PropertyGrid.ValueEditorTemplateSelector != this)
        {
            var template = PropertyGrid.ValueEditorTemplateSelector.SelectTemplate(item, container);
            if (template != null)
                return template;
        }

        foreach (var template in DataTemplates)
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
}
