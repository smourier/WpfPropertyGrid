namespace WpfPropertyGrid.Samples.Utilities;

public class BooleanValueProvider : MarkupExtension
{
    public bool IsNullable { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var items = new ObservableCollection<KeyValuePair<string, object?>>
        {
            new("Yes", true),
            new("No", false)
        };

        if (IsNullable)
        {
            items.Add(new KeyValuePair<string, object?>(string.Empty, null));
        }
        return items;
    }
}
