namespace WpfPropertyGrid.Utilities;

[Flags]
public enum ColorPickerOptions
{
    None = 0x0,
    NormalizedInputs = 0x1, // argb & hsv are floating points between 0 and 1
}
