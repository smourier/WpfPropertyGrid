namespace WpfPropertyGrid;

public class PropertyGridEnumProperty(PropertyGridDataProvider provider) : PropertyGridProperty(provider)
{
    public override void OnValueChanged()
    {
        base.OnValueChanged();
        EnumAttributes.Properties.Clear();
        if (PropertyType != null)
        {
            foreach (var fi in PropertyType.GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                if (fi.Name.Equals(string.Format("{0}", base.Value)))
                {
                    PropertyGridDataProvider.AddDynamicProperties(fi.GetCustomAttributes<PropertyGridAttribute>(), EnumAttributes);
                }
            }
        }
    }

    public virtual Utilities.DynamicObject EnumAttributes { get; private set; } = provider.CreateDynamicObject() ?? throw new NotSupportedException();
}