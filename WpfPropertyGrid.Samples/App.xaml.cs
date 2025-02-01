namespace WpfPropertyGrid.Samples;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
#if DEBUG
        WpfTracing.Enable();
#endif
    }

    private void OnEditorWindowCloseCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
    private void OnEditorWindowCloseExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var window = (Window)sender;
        window.DialogResult = false;
        window.Close();
    }
}
