namespace WpfPropertyGrid.Samples;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
#if DEBUG
        WpfTracing.Enable();
#endif

        // WPF doesn't (currently?) do this automatically 
        var theme = WpfUtilities.GetTheme(this);
        switch (theme)
        {
            case WpfTheme.Dark:
                Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/WpfPropertyGrid;component/Themes/Fluent.Dark.xaml", UriKind.RelativeOrAbsolute) });
                break;

            case WpfTheme.Light:
                Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/WpfPropertyGrid;component/Themes/Fluent.Light.xaml", UriKind.RelativeOrAbsolute) });
                break;
        }
    }

    private void OnEditorWindowCloseCanExecute(object? sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
    private void OnEditorWindowCloseExecuted(object? sender, ExecutedRoutedEventArgs e)
    {
        var window = (Window)sender!;
        window.DialogResult = false;
        window.Close();
    }
}
