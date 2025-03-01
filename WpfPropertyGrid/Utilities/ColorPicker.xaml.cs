using System.Windows.Media.Imaging;

namespace WpfPropertyGrid.Utilities;

public partial class ColorPicker : UserControl
{
    public static DependencyProperty SelectedColorProperty { get; } =
        DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(ColorPicker),
        new FrameworkPropertyMetadata(Colors.Black, FrameworkPropertyMetadataOptions.AffectsRender, (s, e) => ((ColorPicker)s).OnSelectedColorChanged((Color)e.OldValue, (Color)e.NewValue)));

    public static DependencyProperty OptionsProperty { get; } =
        DependencyProperty.Register(nameof(Options), typeof(ColorPickerOptions), typeof(ColorPicker),
        new FrameworkPropertyMetadata(ColorPickerOptions.None, FrameworkPropertyMetadataOptions.AffectsRender, (s, e) => ((ColorPicker)s).OnOptionsChanged((ColorPickerOptions)e.OldValue, (ColorPickerOptions)e.NewValue)));

    public ColorPicker()
    {
        InitializeComponent();
        UpdateHue();
        UpdateValueSaturation();
    }

    public Color SelectedColor { get => (Color)GetValue(SelectedColorProperty); set => SetValue(SelectedColorProperty, value); }
    public ColorPickerOptions Options { get => (ColorPickerOptions)GetValue(OptionsProperty); set => SetValue(OptionsProperty, value); }

    protected virtual void OnSelectedColorChanged(Color oldValue, Color newValue)
    {
        UpdateValueSaturation();
    }

    protected virtual void OnOptionsChanged(ColorPickerOptions oldValue, ColorPickerOptions newValue)
    {
    }

    private void UpdateSelectedColor(Color color)
    {
        if (SelectedColor == color)
            return;

        SelectedColor = color;
    }

    private void UpdateInputs(Color color)
    {
        Dispatcher.BeginInvoke(() =>
        {
            Argb_A.Text = color.A.ToString();
            Argb_R.Text = color.R.ToString();
            Argb_G.Text = color.G.ToString();
            Argb_B.Text = color.B.ToString();
        });
    }

    private void UpdateValueSaturation()
    {
        var hue = Hsv.From(SelectedColor).Hue;
        var bmp = new WriteableBitmap((int)ValueSaturation.Width, (int)ValueSaturation.Height, 96, 96, PixelFormats.Bgra32, null);
        bmp.Lock();
        for (var y = 0; y < bmp.PixelHeight; y++)
        {
            for (var x = 0; x < bmp.PixelWidth; x++)
            {
                var buffer = bmp.BackBuffer + bmp.BackBufferStride * y + 4 * x;
                var hsv = new Hsv(hue, x / (float)bmp.PixelWidth, 1 - y / (float)bmp.PixelHeight);
                Marshal.WriteInt32(buffer, hsv.ToArgb());
            }
        }
        bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
        bmp.Unlock();
        ValueSaturation.Source = bmp;
    }

    private void UpdateHue()
    {
        var bmp = new WriteableBitmap((int)Hue.Width, (int)Hue.Height, 96, 96, PixelFormats.Bgra32, null);
        bmp.Lock();
        for (var y = 0; y < bmp.PixelHeight; y++)
        {
            for (var x = 0; x < bmp.PixelWidth; x++)
            {
                var buffer = bmp.BackBuffer + bmp.BackBufferStride * y + 4 * x;
                var hsv = new Hsv(y / (float)bmp.PixelHeight, 1, 1);
                Marshal.WriteInt32(buffer, hsv.ToArgb());
            }
        }

        bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
        bmp.Unlock();
        Hue.Source = bmp;
    }

    private void UpdateValueSaturationHandle(Point position)
    {
        position.X = Math.Max(0, Math.Min(position.X, ValueSaturationCanvas.ActualWidth));
        position.Y = Math.Max(0, Math.Min(position.Y, ValueSaturationCanvas.ActualHeight));

        Canvas.SetLeft(ValueSaturationHandle, position.X);
        Canvas.SetTop(ValueSaturationHandle, position.Y);
        var hsv = Hsv.From(SelectedColor);
        hsv.Saturation = (float)(position.X / HueCanvas.ActualWidth);
        hsv.Value = 1 - (float)(position.Y / HueCanvas.ActualHeight);
        var color = hsv.ToColor();
        UpdateSelectedColor(color);
        UpdateInputs(color);
    }

    private void UpdateHueHandle(Point position)
    {
        position.Y = Math.Max(0, Math.Min(position.Y, HueCanvas.ActualHeight));
        Canvas.SetTop(HueHandle, position.Y);
        var hsv = Hsv.From(SelectedColor);
        hsv.Hue = (float)(position.Y / HueCanvas.ActualHeight);
        var color = hsv.ToColor();
        UpdateSelectedColor(color);
        UpdateInputs(color);
    }

    private void OnValueSaturationMouseDown(object sender, MouseButtonEventArgs e)
    {
        UpdateValueSaturationHandle(e.GetPosition(ValueSaturationCanvas));
        ((UIElement)sender).CaptureMouse();
    }

    private void OnValueSaturationMouseUp(object sender, MouseButtonEventArgs e)
    {
        UpdateValueSaturationHandle(e.GetPosition(ValueSaturationCanvas));
        ((UIElement)sender).ReleaseMouseCapture();
    }

    private void OnValueSaturationMouseMove(object sender, MouseEventArgs e)
    {
        if (!((UIElement)sender).IsMouseCaptured)
            return;

        UpdateValueSaturationHandle(e.GetPosition(ValueSaturationCanvas));
    }

    private void OnHueMouseDown(object sender, MouseButtonEventArgs e)
    {
        UpdateHueHandle(e.GetPosition(HueCanvas));
        ((UIElement)sender).CaptureMouse();
    }

    private void OnHueMouseUp(object sender, MouseButtonEventArgs e)
    {
        UpdateHueHandle(e.GetPosition(HueCanvas));
        ((UIElement)sender).ReleaseMouseCapture();
    }

    private void OnHueMouseMove(object sender, MouseEventArgs e)
    {
        if (!((UIElement)sender).IsMouseCaptured)
            return;

        UpdateHueHandle(e.GetPosition(HueCanvas));
    }
}
