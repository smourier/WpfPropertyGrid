namespace WpfPropertyGrid.Samples.Utilities;

public static class WpfUtilities
{
    public static readonly DependencyProperty BindableSourceProperty =
        DependencyProperty.RegisterAttached("BindableSource", typeof(string), typeof(WpfUtilities), new UIPropertyMetadata(null, BindableSourcePropertyChanged));

    public static readonly DependencyProperty BindablePasswordProperty =
        DependencyProperty.RegisterAttached("BindablePassword", typeof(SecureString), typeof(WpfUtilities), new FrameworkPropertyMetadata(null, OnPasswordPropertyChanged));

    public static readonly DependencyProperty BindPasswordProperty =
        DependencyProperty.RegisterAttached("BindPassword", typeof(bool), typeof(WpfUtilities), new PropertyMetadata(false, BindPassword));

    private static readonly DependencyProperty UpdatingPasswordProperty =
        DependencyProperty.RegisterAttached("UpdatingPassword", typeof(bool), typeof(WpfUtilities));

    public static string GetBindableSource(DependencyObject obj) => (string)obj.GetValue(BindableSourceProperty);
    public static void SetBindableSource(DependencyObject obj, string value) => obj.SetValue(BindableSourceProperty, value);
    public static void BindableSourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763) && o is WebView2 browser)
        {
            var uri = e.NewValue as string;
            browser.Source = !string.IsNullOrEmpty(uri) ? new Uri(uri) : null;
        }
    }

    public static string? ConvertToUnsecureString(this SecureString securePassword)
    {
        ArgumentNullException.ThrowIfNull(securePassword);
        var unmanagedString = nint.Zero;
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

    // https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/ui/apply-windows-themes
    public static bool IsColorLight(Windows.UI.Color color) => ((5 * color.G) + (2 * color.R) + color.B) > (8 * 128);

    public static bool? IsDarkMode()
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 10240))
            return IsColorLight(new UISettings().GetColorValue(UIColorType.Foreground));

        return null;
    }

#pragma warning disable WPF0001 // Type is for evaluation purposes only and is subject to change or removal in future updates
    public static WpfTheme GetTheme(ThemeMode mode)
    {
        if (mode == ThemeMode.System)
            return IsDarkMode() == true ? WpfTheme.Dark : WpfTheme.Light;

        if (mode == ThemeMode.Dark)
            return WpfTheme.Dark;

        if (mode == ThemeMode.Light)
            return WpfTheme.Light;

        return WpfTheme.None;
    }

    // returns only none or dark or light
    public static WpfTheme GetTheme(Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        return GetTheme(application.ThemeMode);
    }

    public static WpfTheme GetTheme(DependencyObject obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        var tm = Window.GetWindow(obj).ThemeMode;
        if (tm == ThemeMode.None)
            return GetTheme(Application.Current);

        return GetTheme(tm);
    }
#pragma warning restore WPF0001 // Type is for evaluation purposes only and is subject to change or removal in future updates
}
