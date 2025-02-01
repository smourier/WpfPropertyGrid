namespace WpfPropertyGrid;

public interface IPropertyGridEditor
{
    bool SetContext(PropertyGridProperty property, object? parameter);
}