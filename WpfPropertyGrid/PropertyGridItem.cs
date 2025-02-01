namespace WpfPropertyGrid
{
    public class PropertyGridItem : AutoObject
    {
        public PropertyGridItem()
        {
            IsChecked = false;
        }

        public virtual bool IsUnset { get => GetProperty<bool>(); set => SetProperty(value); }
        public virtual bool IsZero { get => GetProperty<bool>(); set => SetProperty(value); }
        public virtual string? Name { get => GetProperty<string>(); set => SetProperty(value); }
        public virtual object? Value { get => GetProperty<object>(); set => SetProperty(value); }
        public virtual bool? IsChecked { get => GetProperty<bool?>(); set => SetProperty(value); }
        public virtual PropertyGridProperty? Property { get => GetProperty<PropertyGridProperty>(); set => SetProperty(value); }

        public override string ToString() => Name ?? string.Empty;
    }
}