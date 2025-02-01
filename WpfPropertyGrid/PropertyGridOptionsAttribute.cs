namespace WpfPropertyGrid;

public class PropertyGridOptionsAttribute : Attribute
{
    public PropertyGridOptionsAttribute()
    {
        EnumSeparator = ", ";
    }

    public string[]? EnumNames { get; set; }
    public object[]? EnumValues { get; set; }
    public bool IsEnum { get; set; }
    public bool IsFlagsEnum { get; set; }
    public int EnumMaxPower { get; set; }
    public bool CollectionEditorHasOnlyOneColumn { get; set; }
    public int SortOrder { get; set; }
    public string? EditorDataTemplatePropertyPath { get; set; }
    public string? EditorDataTemplateSelectorPropertyPath { get; set; }
    public Type? EditorType { get; set; }
    public string? EditorResourceKey { get; set; }
    public object? EditorDataTemplateResourceKey { get; set; }
    public Type? PropertyType { get; set; }
    public bool ForceReadWrite { get; set; }
    public bool HasDefaultValue { get; set; }
    public bool ForcePropertyChanged { get; set; }
    public object? DefaultValue { get; set; }
    public string? EnumSeparator { get; set; }

    public static DataTemplate? SelectTemplate(PropertyGridProperty property, object? item, DependencyObject container)
    {
        ArgumentNullException.ThrowIfNull(property);

        var att = FromProperty(property);
        if (att == null)
            return null;

        if (att.EditorDataTemplateResourceKey != null)
        {
            if (Application.Current != null)
            {
                var dt = (DataTemplate)Application.Current.TryFindResource(att.EditorDataTemplateResourceKey);
                if (dt != null)
                    return dt;
            }

            if (container is FrameworkElement fe)
            {
                var obj = fe.TryFindResource(att.EditorDataTemplateResourceKey);
                if (obj is DataTemplate dt)
                    return dt;
            }

            return null;
        }

        if (att.EditorType != null)
        {
            var editor = Activator.CreateInstance(att.EditorType);
            if (att.EditorDataTemplateSelectorPropertyPath != null)
            {
                var dts = DataBindingEvaluator.GetPropertyValue(editor, att.EditorDataTemplateSelectorPropertyPath) as DataTemplateSelector;
                return dts?.SelectTemplate(item, container);
            }

            if (att.EditorDataTemplatePropertyPath != null)
                return DataBindingEvaluator.GetPropertyValue(editor, att.EditorDataTemplatePropertyPath) as DataTemplate;

            if (editor is ContentControl cc)
            {
                if (cc.ContentTemplateSelector != null)
                {
                    var template = cc.ContentTemplateSelector.SelectTemplate(item, container);
                    if (template != null)
                        return template;
                }

                return cc.ContentTemplate;
            }

            if (editor is ContentPresenter cp)
            {
                if (cp.ContentTemplateSelector != null)
                {
                    var template = cp.ContentTemplateSelector.SelectTemplate(item, container);
                    if (template != null)
                        return template;
                }

                return cp.ContentTemplate;
            }
        }
        return null;
    }

    public static PropertyGridOptionsAttribute? FromProperty(PropertyGridProperty property)
    {
        ArgumentNullException.ThrowIfNull(property);

        var att = property.Options;
        if (att != null)
            return att;

        if (property.Descriptor != null)
        {
            att = property.Descriptor.Attributes.OfType<PropertyGridOptionsAttribute>().FirstOrDefault();
        }
        return att;
    }
}