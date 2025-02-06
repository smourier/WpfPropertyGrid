namespace WpfPropertyGrid.Utilities;

public class DictionaryObjectPropertyRollbackEventArgs : EventArgs
{
    public DictionaryObjectPropertyRollbackEventArgs(string propertyName, DictionaryObjectProperty? existingProperty, object? invalidValue)
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        // existingProperty may be null

        PropertyName = propertyName;
        ExistingProperty = existingProperty;
        InvalidValue = invalidValue;
    }

    public string PropertyName { get; }
    public DictionaryObjectProperty? ExistingProperty { get; }
    public object? InvalidValue { get; }
}
