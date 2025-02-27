namespace WpfPropertyGrid.Utilities;

public class ValidateTextEventArgs(string? text) : CancelEventArgs
{
    public string? Text { get; } = text;

    public virtual string? ReplacementText { get; set; }
}