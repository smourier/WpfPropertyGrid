namespace WpfPropertyGrid.Utilities;

public class DictionaryObjectPropertyChangedEventArgs : PropertyChangedEventArgs
{
    public DictionaryObjectPropertyChangedEventArgs(string propertyName, DictionaryObjectProperty? existingProperty, DictionaryObjectProperty newProperty)
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
}
