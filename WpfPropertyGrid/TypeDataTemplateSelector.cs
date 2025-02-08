namespace WpfPropertyGrid;

[ContentProperty("DataTemplates")]
public class TypeDataTemplateSelector : DataTemplateSelector
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ObservableCollection<DataTemplate> DataTemplates { get; } = [];

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        foreach (var template in DataTemplates.Where(dt => dt.DataType is Type))
        {
            var type = template.DataType as Type;
            if (type == null)
                continue;

            if (item == null)
            {
                if (!type.IsValueType)
                    return template;
            }
            else
            {
                if (type.IsInstanceOfType(item))
                    return template;
            }
        }
        return DataTemplates.FirstOrDefault(dt => dt.DataType == null);
    }
}