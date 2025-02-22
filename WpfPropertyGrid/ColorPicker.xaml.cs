using System.Windows.Media.Imaging;

namespace WpfPropertyGrid;

public partial class ColorPicker : UserControl
{
    public static DependencyProperty SelectedColorProperty { get; } =
        DependencyProperty.Register(nameof(SelectedColor), typeof(object), typeof(ColorPicker),
        new FrameworkPropertyMetadata(Colors.Black, FrameworkPropertyMetadataOptions.AffectsRender, (s, e) => ((ColorPicker)s).OnSelectedColorChanged(e)));

    public ColorPicker()
    {
        InitializeComponent();
    }

    public Color SelectedColor { get => (Color)GetValue(SelectedColorProperty); set => SetValue(SelectedColorProperty, value); }

    protected virtual void OnSelectedColorChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateValueSaturationSquare();
    }

    private void UpdateValueSaturationSquare()
    {
        var hsv = Hsv.From(SelectedColor);
        var size = 100;
        var step = 1f / size;
        var bmp = new WriteableBitmap(size, size, 96, 96, PixelFormats.Bgra32, null);
        bmp.Lock();
        for (float y = 0; y < 1.0f; y += step)
        {
            for (float x = 0; x < 1.0f; x += step)
            {
                Marshal.WriteInt32(bmp.BackBuffer, new Hsv(hsv.Hue, x, y).ToArgb());
            }
        }
        bmp.Unlock();
        svSquare.Source = bmp;
    }
}
