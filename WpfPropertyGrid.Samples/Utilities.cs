namespace WpfPropertyGrid.Samples;

public static class Utilities
{
    public static readonly DependencyProperty BindableSourceProperty =
        DependencyProperty.RegisterAttached("BindableSource", typeof(string), typeof(Utilities), new UIPropertyMetadata(null, BindableSourcePropertyChanged));

    public static string GetBindableSource(DependencyObject obj) => (string)obj.GetValue(BindableSourceProperty);

    public static void SetBindableSource(DependencyObject obj, string value) => obj.SetValue(BindableSourceProperty, value);

    public static void BindableSourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        if (o is WebBrowser browser)
        {
            var uri = e.NewValue as string;
            browser.Source = !string.IsNullOrEmpty(uri) ? new Uri(uri) : null;
        }
    }

    public static readonly DependencyProperty BindablePasswordProperty = DependencyProperty.RegisterAttached("BindablePassword", typeof(SecureString), typeof(Utilities), new FrameworkPropertyMetadata(null, OnPasswordPropertyChanged));

    public static readonly DependencyProperty BindPasswordProperty = DependencyProperty.RegisterAttached("BindPassword", typeof(bool), typeof(Utilities), new PropertyMetadata(false, BindPassword));

    private static readonly DependencyProperty UpdatingPasswordProperty = DependencyProperty.RegisterAttached("UpdatingPassword", typeof(bool), typeof(Utilities));

    public static void SetBindPassword(DependencyObject dp, bool value) => dp.SetValue(BindPasswordProperty, value);

    public static bool GetBindPassword(DependencyObject dp) => (bool)dp.GetValue(BindPasswordProperty);

    public static string GetBindablePassword(DependencyObject dp) => (string)dp.GetValue(BindablePasswordProperty);

    public static void SetBindablePassword(DependencyObject dp, SecureString value) => dp.SetValue(BindablePasswordProperty, value);

    private static bool GetUpdatingPassword(DependencyObject dp) => (bool)dp.GetValue(UpdatingPasswordProperty);

    private static void SetUpdatingPassword(DependencyObject dp, bool value) => dp.SetValue(UpdatingPasswordProperty, value);

    private static void OnPasswordPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not PasswordBox passwordBox)
            return;

        passwordBox.PasswordChanged -= PasswordChanged;
        if (!GetUpdatingPassword(passwordBox))
        {
            passwordBox.Password = (string)e.NewValue;
        }
        passwordBox.PasswordChanged += PasswordChanged;
    }

    private static void BindPassword(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not PasswordBox passwordBox)
            return;

        if ((bool)e.OldValue)
        {
            passwordBox.PasswordChanged -= PasswordChanged;
        }

        if ((bool)e.NewValue)
        {
            passwordBox.PasswordChanged += PasswordChanged;
        }
    }

    private static void PasswordChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not PasswordBox passwordBox)
            return;

        SetUpdatingPassword(passwordBox, true);
        SetBindablePassword(passwordBox, passwordBox.SecurePassword);
        SetUpdatingPassword(passwordBox, false);
    }

    public static string ConvertToUnsecureString(this SecureString securePassword)
    {
        ArgumentNullException.ThrowIfNull(securePassword);

        IntPtr unmanagedString = IntPtr.Zero;
        try
        {
            unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
            return Marshal.PtrToStringUni(unmanagedString);
        }
        finally
        {
            Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
        }
    }
}

public class BooleanValueProvider : MarkupExtension
{
    public bool IsNullable { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var items = new ObservableCollection<KeyValuePair<string, object>>
        {
            new("Yes", true),
            new("No", false)
        };
        if (IsNullable)
        {
            items.Add(new KeyValuePair<string, object>("", null));
        }
        return items;
    }
}
