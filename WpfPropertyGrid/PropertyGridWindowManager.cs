namespace WpfPropertyGrid;

public class PropertyGridWindowManager
{
    public static readonly DependencyProperty OptionsProperty =
        DependencyProperty.RegisterAttached("Options", typeof(PropertyGridWindowOptions), typeof(PropertyGridWindowManager), new PropertyMetadata(PropertyGridWindowOptions.None, OnOptionsChanged));

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public static PropertyGridWindowOptions GetOptions(DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);

        return (PropertyGridWindowOptions)element.GetValue(OptionsProperty);
    }

    public static void SetOptions(DependencyObject element, PropertyGridWindowOptions value)
    {
        ArgumentNullException.ThrowIfNull(element);

        element.SetValue(OptionsProperty, value);
    }

    private static void OnOptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }
}