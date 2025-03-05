using System.Windows.Media.Imaging;

namespace WpfPropertyGrid.Utilities;

public partial class ColorPicker : UserControl
{
    public static DependencyProperty SelectedColorProperty { get; } =
        DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(ColorPicker),
        new FrameworkPropertyMetadata(Colors.Black, FrameworkPropertyMetadataOptions.AffectsRender, (s, e) => ((ColorPicker)s).OnSelectedColorChanged((Color)e.OldValue, (Color)e.NewValue)));

    private int _updating;

    public ColorPicker()
    {
        InitializeComponent();
        UpdateValueSaturationImage(Colors.Black);
        UpdateHueImage();
        Dispatcher.BeginInvoke(UpdateSelectedColor);
    }

    public Color SelectedColor { get => (Color)GetValue(SelectedColorProperty); set => SetValue(SelectedColorProperty, value); }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        UpdateValueSaturationImage(SelectedColor);
        UpdateHueImage();
    }

    protected virtual void OnSelectedColorChanged(Color oldValue, Color newValue) => WithoutReentrency(UpdateSelectedColor);

    private void UpdateSelectedColor()
    {
        var color = SelectedColor;
        UpdateValueSaturationHandle(color);
        UpdateHueHandle(color);
        UpdateAlphaHandle(color);
        UpdateInputs(color);
    }

    private void UpdateValueSaturationImage(Color color)
    {
        var hue = Hsv.From(color).Hue;
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

    private void UpdateHueImage()
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

    private void WithoutReentrency(Action action)
    {
        if (_updating > 0)
            return;

        _updating++;
        Dispatcher.BeginInvoke(() =>
        {
            try
            {
                action();
            }
            finally
            {
                _updating--;
            }
        });
    }

    private void UpdateComplementary(Color color)
    {
        // use complementary for hue handle color
        var hsv = Hsv.From(color);
        hsv.Saturation = 1;
        hsv.Value = 1;
        ValueSaturationHandle.Fill = new SolidColorBrush(hsv.Complementary);
    }

    private void UpdateInputs(Color color)
    {
        UpdateArgb(color);
        UpdateHsv(color);
        UpdateHex(color);
    }

    private void UpdateArgb(Color color)
    {
        Argb_R.Text = color.R.ToString();
        Argb_G.Text = color.G.ToString();
        Argb_B.Text = color.B.ToString();
        Argb_A.Text = color.A.ToString();
    }

    private void UpdateHsv(Color color)
    {
        var hsv = Hsv.From(color);
        Hsv_H.Text = hsv.Hue.ToString("F3");
        Hsv_S.Text = hsv.Saturation.ToString("F3");
        Hsv_V.Text = hsv.Value.ToString("F3");
    }

    private void UpdateHex(Color color)
    {
        Hex.Text = ColorTextBox.Format(color);
    }

    private void UpdateValueSaturationHandle(Color color)
    {
        Dispatcher.BeginInvoke(() => UpdateValueSaturationImage(color));
        UpdateComplementary(color);
        var hsv = Hsv.From(color);
        Canvas.SetLeft(ValueSaturationHandle, hsv.Saturation * ValueSaturationCanvas.ActualWidth);
        Canvas.SetTop(ValueSaturationHandle, (1 - hsv.Value) * ValueSaturationCanvas.ActualHeight);
    }

    private void UpdateHueHandle(Color color)
    {
        var hsv = Hsv.From(color);
        Canvas.SetTop(HueHandle, hsv.Hue * HueCanvas.ActualHeight);
    }

    private void UpdateAlphaHandle(Color color)
    {
        Canvas.SetTop(AlphaHandle, (1 - color.ScA) * AlphaCanvas.ActualHeight);
        Alpha.Fill = new SolidColorBrush(color);
    }

    private void UpdateValueSaturationHandle(Point position)
    {
        position.X = Math.Max(0, Math.Min(position.X, ValueSaturationCanvas.ActualWidth));
        position.Y = Math.Max(0, Math.Min(position.Y, ValueSaturationCanvas.ActualHeight));

        Canvas.SetLeft(ValueSaturationHandle, position.X);
        Canvas.SetTop(ValueSaturationHandle, position.Y);

        var color = SelectedColor;
        WithoutReentrency(() =>
        {
            var hsv = Hsv.From(color);
            hsv.Saturation = (float)(position.X / ValueSaturationCanvas.ActualWidth);
            hsv.Value = 1 - (float)(position.Y / ValueSaturationCanvas.ActualHeight);
            color = hsv.ToColor(color.ScA);
            UpdateAlphaHandle(color);
            UpdateInputs(color);
            SelectedColor = color;
        });
    }

    private void UpdateHueHandle(Point position)
    {
        var pos = Math.Max(0, Math.Min(position.Y, HueCanvas.ActualHeight));
        Canvas.SetTop(HueHandle, pos);
        var color = SelectedColor;

        WithoutReentrency(() =>
        {
            var hsv = Hsv.From(color);
            hsv.Hue = (float)(pos / HueCanvas.ActualHeight);
            color = hsv.ToColor(color.ScA);
            UpdateValueSaturationHandle(color);
            UpdateAlphaHandle(color);
            UpdateInputs(color);
            SelectedColor = color;
        });
    }

    private void UpdateAlphaHandle(Point position)
    {
        var pos = Math.Max(0, Math.Min(position.Y, AlphaCanvas.ActualHeight));
        Canvas.SetTop(AlphaHandle, pos);
        var color = SelectedColor;

        WithoutReentrency(() =>
        {
            color.ScA = 1 - (float)(pos / AlphaCanvas.ActualHeight);
            UpdateInputs(color);
            SelectedColor = color;
        });
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

    private void OnAlphaMouseDown(object sender, MouseButtonEventArgs e)
    {
        UpdateAlphaHandle(e.GetPosition(AlphaCanvas));
        ((UIElement)sender).CaptureMouse();
    }

    private void OnAlphaMouseUp(object sender, MouseButtonEventArgs e)
    {
        UpdateAlphaHandle(e.GetPosition(AlphaCanvas));
        ((UIElement)sender).ReleaseMouseCapture();
    }

    private void OnAlphaMouseMove(object sender, MouseEventArgs e)
    {
        if (!((UIElement)sender).IsMouseCaptured)
            return;

        UpdateAlphaHandle(e.GetPosition(AlphaCanvas));
    }

    private void UpdateFromArgb(Color color) => WithoutReentrency(() =>
    {
        UpdateValueSaturationHandle(color);
        UpdateHueHandle(color);
        UpdateAlphaHandle(color);
        UpdateHsv(color);
        UpdateHex(color);
        SelectedColor = color;
    });

    private void UpdateFromHsv(Color color) => WithoutReentrency(() =>
    {
        UpdateValueSaturationHandle(color);
        UpdateHueHandle(color);
        UpdateArgb(color);
        UpdateHex(color);
        SelectedColor = color;
    });

    private void OnRedChanged(object sender, TextChangedEventArgs e)
    {
        var color = SelectedColor;
        var newColor = Color.FromArgb(color.A, Argb_R.Value, color.G, color.B);
        UpdateFromArgb(newColor);
    }

    private void OnGreenChanged(object sender, TextChangedEventArgs e)
    {
        var color = SelectedColor;
        var newColor = Color.FromArgb(color.A, color.R, Argb_G.Value, color.B);
        UpdateFromArgb(newColor);
    }

    private void OnBlueChanged(object sender, TextChangedEventArgs e)
    {
        var color = SelectedColor;
        var newColor = Color.FromArgb(color.A, color.R, color.G, Argb_B.Value);
        UpdateFromArgb(newColor);
    }

    private void OnAlphaChanged(object sender, TextChangedEventArgs e)
    {
        var color = SelectedColor;
        var newColor = Color.FromArgb(Argb_A.Value, color.R, color.G, color.B);
        UpdateFromArgb(newColor);
    }

    private void OnHueChanged(object sender, TextChangedEventArgs e)
    {
        var color = SelectedColor;
        var hsv = Hsv.From(color);
        hsv.Hue = Hsv_H.Value;
        UpdateFromHsv(hsv.ToColor(color.ScA));
    }

    private void OnSaturationChanged(object sender, TextChangedEventArgs e)
    {
        var color = SelectedColor;
        var hsv = Hsv.From(color);
        hsv.Saturation = Hsv_S.Value;
        UpdateFromHsv(hsv.ToColor(color.ScA));
    }

    private void OnValueChanged(object sender, TextChangedEventArgs e)
    {
        var color = SelectedColor;
        var hsv = Hsv.From(color);
        hsv.Value = Hsv_V.Value;
        UpdateFromHsv(hsv.ToColor(color.ScA));
    }

    private void OnHexChanged(object sender, TextChangedEventArgs e)
    {
        var color = Hex.Value;
        WithoutReentrency(() =>
        {
            UpdateValueSaturationHandle(color);
            UpdateHueHandle(color);
            UpdateAlphaHandle(color);
            UpdateArgb(color);
            UpdateHsv(color);
            SelectedColor = color;
        });
    }
}
