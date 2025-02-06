namespace WpfPropertyGrid.Utilities;

public class DictionaryObjectPropertyChangingEventArgs : PropertyChangingEventArgs
{
    public DictionaryObjectPropertyChangingEventArgs(string propertyName, DictionaryObjectProperty? existingProperty, DictionaryObjectProperty newProperty)
        : base(propertyName)
    {
        ArgumentNullException.ThrowIfNull(propertyName);
        ArgumentNullException.ThrowIfNull(newProperty);

        // existingProperty may be null

        ExistingProperty = existingProperty;
        NewProperty = newProperty;
    }

    public DictionaryObjectProperty? ExistingProperty { get; }
    public DictionaryObjectProperty NewProperty { get; }
    public virtual bool Cancel { get; set; }
}
