namespace WpfPropertyGrid;

public class PropertyGridEventArgs(PropertyGridProperty property, object? context = null) : CancelEventArgs
{
    public PropertyGridProperty Property { get; } = property;
    public object? Context { get; set; } = context;
}