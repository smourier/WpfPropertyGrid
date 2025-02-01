namespace WpfPropertyGrid;

[ContentProperty("DataTemplates")]
public class TypeDataTemplateSelector : DataTemplateSelector
{
    public TypeDataTemplateSelector()
    {
        DataTemplates = [];
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ObservableCollection<DataTemplate> DataTemplates { get; private set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        foreach (var dt in DataTemplates.Where(dt => dt.DataType is Type))
        {
            var type = dt.DataType as Type;
            if (type == null)
                continue;

            if (item == null)
            {
                if (!type.IsValueType)
                    return dt;
            }
            else
            {
                if (type.IsInstanceOfType(item))
                    return dt;
            }
        }
        return DataTemplates.FirstOrDefault(dt => dt.DataType == null);
    }
}