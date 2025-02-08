namespace WpfPropertyGrid
{
    public class PropertyGridItem : DictionaryObject
    {
        public PropertyGridItem()
        {
            IsChecked = false;
        }

        public new bool DictionaryObjectRaiseOnPropertyChanged { get => base.DictionaryObjectRaiseOnPropertyChanged; set => base.DictionaryObjectRaiseOnPropertyChanged = value; }
        public new bool DictionaryObjectRaiseOnPropertyChanging { get => base.DictionaryObjectRaiseOnPropertyChanging; set => base.DictionaryObjectRaiseOnPropertyChanging = value; }
        public new bool DictionaryObjectRaiseOnErrorsChanged { get => base.DictionaryObjectRaiseOnErrorsChanged; set => base.DictionaryObjectRaiseOnErrorsChanged = value; }

        public virtual bool IsUnset { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }
        public virtual bool IsZero { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }
        public virtual string? Name { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }
        public virtual object? Value { get => DictionaryObjectGetPropertyValue<object>(); set => DictionaryObjectSetPropertyValue(value); }
        public virtual bool? IsChecked { get => DictionaryObjectGetPropertyValue<bool?>(); set => DictionaryObjectSetPropertyValue(value); }
        public virtual PropertyGridProperty? Property { get => DictionaryObjectGetPropertyValue<PropertyGridProperty>(); set => DictionaryObjectSetPropertyValue(value); }

        public override string ToString() => Name ?? string.Empty;
    }
}