namespace WpfPropertyGrid.Samples;

/// <summary>
/// Interaction logic for AddressListEditorWindow.xaml
/// </summary>
public partial class AddressListEditorWindow : Window
{
    public AddressListEditorWindow()
    {
        InitializeComponent();
    }

    private void NewCommandBinding_OnCanExecute(object? sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;

    private void NewCommandBinding_OnExecuted(object? sender, ExecutedRoutedEventArgs e)
    {
        var cvs = CollectionViewSource.GetDefaultView(EditorSelector.ItemsSource);
        if (cvs == null)
            return;

        if (cvs.SourceCollection is ICollection<Address> collection)
        {
            Address address = new()
            {
                Line1 = "Empty"
            };
            collection.Add(address);
            cvs.MoveCurrentToLast();
        }
    }

    private void DeleteCommandBinding_OnCanExecute(object? sender, CanExecuteRoutedEventArgs e)
    {
        var cvs = CollectionViewSource.GetDefaultView(EditorSelector.ItemsSource);
        if (cvs == null)
            return;

        e.CanExecute = cvs.CurrentItem != null;
    }

    private void DeleteCommandBinding_OnExecuted(object? sender, ExecutedRoutedEventArgs e)
    {
        var cvs = CollectionViewSource.GetDefaultView(EditorSelector.ItemsSource);
        if (cvs == null)
            return;

        if (cvs.CurrentItem is not Address currentItem)
            return;

        if (cvs.SourceCollection is ICollection<Address> collection)
        {
            collection.Remove(currentItem);
        }
    }

    protected virtual void OnEditorWindowCloseExecuted(object? sender, ExecutedRoutedEventArgs e)
    {
        Window window = (Window)sender;
        if (window.DataContext is PropertyGridProperty prop)
        {
            prop.Executed(sender, e);
            if (e.Handled)
                return;
        }
        window.Close();
    }

    protected virtual void OnEditorWindowCloseCanExecute(object? sender, CanExecuteRoutedEventArgs e)
    {
        Window window = (Window)sender;
        if (window.DataContext is PropertyGridProperty prop)
        {
            prop.CanExecute(sender, e);
            if (e.Handled)
                return;
        }
        e.CanExecute = true;
    }
}
