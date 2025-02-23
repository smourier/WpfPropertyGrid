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
        //    UpdateValueSaturationSquare();
        //}

        //private void UpdateValueSaturationSquare()
        //{
        //    var color = Hsv.From(SelectedColor);
        //    var size = 100;
        //    var bmp = new WriteableBitmap(size, size, 96, 96, PixelFormats.Bgra32, null);
        //    bmp.Lock();
        //    for (var y = 0; y < bmp.PixelHeight; y++)
        //    {
        //        for (var x = 0; x < bmp.PixelWidth; x++)
        //        {
        //            var buffer = bmp.BackBuffer + bmp.BackBufferStride * y + 4 * x;
        //            var hsv = new Hsv(color.Hue, x / (float)size, 1 - y / (float)size);
        //            Marshal.WriteInt32(buffer, hsv.ToArgb());
        //        }
        //    }

        //    bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
        //    bmp.Unlock();
        //    svSquare.Width = size;
        //    svSquare.Height = size;
        //    svSquare.Source = bmp;
    }

    private void OnWheelHandlerMouseDown(object sender, MouseButtonEventArgs e)
    {
        ((UIElement)sender).CaptureMouse();
    }

    private void OnWheelHandlerMouseUp(object sender, MouseButtonEventArgs e)
    {
        ((UIElement)sender).ReleaseMouseCapture();
    }

    private void OnWheelHandlerMouseMove(object sender, MouseEventArgs e)
    {
        if (!((UIElement)sender).IsMouseCaptured)
            return;

        var circlePos = e.GetPosition(Circle);
        var hit = VisualTreeHelper.HitTest(Circle, circlePos);
        if (hit == null)
            return;

        var pos = e.GetPosition(HandleCanvas);
        Canvas.SetLeft(Handle, pos.X);
        Canvas.SetTop(Handle, pos.Y);

        var w = HandleCanvas.ActualWidth;
        var h = HandleCanvas.ActualHeight;

        var angle = 180 + 180 * Math.Atan2(pos.X - w / 2, pos.Y - h / 2) / Math.PI;
        Trace.WriteLine("angle:" + angle);
        var hsv = new Hsv((float)angle, 1, 1);
        SelectedColor = hsv.Color;
    }
}
