namespace WpfPropertyGrid.Samples;

public partial class ColorPicker : UserControl, INotifyPropertyChanged
{
    public static DependencyProperty SelectedColorProperty { get; } =
        DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(ColorPicker),
        new FrameworkPropertyMetadata(Colors.Black, FrameworkPropertyMetadataOptions.AffectsRender, (s, e) => ((ColorPicker)s).OnSelectedColorChanged((Color)e.OldValue, (Color)e.NewValue)));

    public event PropertyChangedEventHandler? PropertyChanged;

    public ColorPicker()
    {
        InitializeComponent();
    }

    public Color SelectedColor { get => (Color)GetValue(SelectedColorProperty); set => SetValue(SelectedColorProperty, value); }

    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        var size = base.ArrangeOverride(arrangeBounds);
        var color = SelectedColor;
        UpdateSelectedColor(color, UpdateSource.WheelHandler);
        UpdateSelectedColor(color, UpdateSource.Inputs);
        return size;
    }

    protected void OnPropertyChanged(string name) => OnPropertyChanged(this, new PropertyChangedEventArgs(name));
    protected virtual void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) => PropertyChanged?.Invoke(sender, e);
    protected virtual void OnSelectedColorChanged(Color oldValue, Color newValue)
    {
    }

    private void OnWheelHandlerMouseDown(object sender, MouseButtonEventArgs e) => ((UIElement)sender).CaptureMouse();
    private void OnWheelHandlerMouseUp(object sender, MouseButtonEventArgs e) => ((UIElement)sender).ReleaseMouseCapture();
    private void OnWheelHandlerMouseMove(object sender, MouseEventArgs e)
    {
        if (!((UIElement)sender).IsMouseCaptured)
            return;

        //Trace.Write("OnWheelHandlerMouseMove");
        var circlePos = e.GetPosition(Circle);
        var hit = VisualTreeHelper.HitTest(Circle, circlePos);
        if (hit == null)
            return;

        var canvasPos = e.GetPosition(HandleCanvas);
        Canvas.SetLeft(Handle, canvasPos.X);
        Canvas.SetTop(Handle, canvasPos.Y);

        canvasPos.X -= Circle.Margin.Left + Circle.Margin.Right;
        canvasPos.Y -= Circle.Margin.Top + Circle.Margin.Bottom;
        var vx = canvasPos.X - Circle.ActualWidth / 2;
        var vy = canvasPos.Y - Circle.ActualWidth / 2;
        var length = Math.Sqrt(vx * vx + vy * vy);
        var angle = (1 + Math.Atan2(vx, vy) / Math.PI) / 2;
        var r = (Circle.ActualWidth - Circle.Margin.Right - Circle.Margin.Left) / 2; // presumes it's a circle
        var saturation = length / r;

        var hsv = new Hsv((float)angle, (float)saturation, 1);
        UpdateSelectedColor(hsv.ToColor(), UpdateSource.WheelHandler);
    }

    private enum UpdateSource
    {
        Inputs,
        WheelHandler
    }

    private UpdateSource? _source;
    private void UpdateSelectedColor(Color color, UpdateSource source)
    {
        _source = source;
        try
        {
            SelectedColor = color;
            switch (source)
            {
                case UpdateSource.Inputs:
                    UpdateWheelHandler(color);
                    break;

                case UpdateSource.WheelHandler:
                    UpdateInputs(color);
                    break;
            }
        }
        finally
        {
            _source = null;
        }
    }

    private void UpdateInputs(Color color)
    {
        ArgbA.Text = color.A.ToString();
        ArgbR.Text = color.R.ToString();
        ArgbG.Text = color.G.ToString();
        ArgbB.Text = color.B.ToString();
        var hsv = Hsv.From(color);
        HsvH.Text = ((int)(hsv.Hue * 360)).ToString();
        HsvS.Text = ((int)(hsv.Saturation * 100)).ToString();
        HsvV.Text = ((int)(hsv.Value * 100)).ToString();
    }

    private void UpdateWheelHandler(Color color)
    {
        //Trace.Write("ResetHandler");

        var hsv = Hsv.From(color);

        var r = (Circle.ActualWidth - Circle.Margin.Right - Circle.Margin.Left) / 2; // presumes it's a circle
        var angle = hsv.Hue * Math.PI * 2;
        var x = Circle.ActualWidth / 2;
        var y = Circle.ActualWidth / 2;
        if (hsv.Saturation != 0)
        {
            x += r * -Math.Sin(angle) * hsv.Saturation;
            y += r * -Math.Cos(angle) * hsv.Saturation;
        }

        x += Circle.Margin.Left;
        y += Circle.Margin.Top;
        Canvas.SetLeft(Handle, x);
        Canvas.SetTop(Handle, y);
    }

    private void HsvH_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_source.HasValue)
            return;

        //Trace.Write("HsvH_TextChanged");
        var h = ((UInt16TextBox)sender).Value / 360f;
        var hsv = Hsv.From(SelectedColor);
        if (h == hsv.Hue)
            return;

        hsv.Hue = h;
        UpdateSelectedColor(hsv.ToColor(), UpdateSource.Inputs);
    }

    private void HsvS_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_source.HasValue)
            return;

        //Trace.Write("HsvS_TextChanged");
        var s = ((ByteTextBox)sender).Value / 100f;
        var hsv = Hsv.From(SelectedColor);
        if (s == hsv.Saturation)
            return;

        hsv.Saturation = s;
        UpdateSelectedColor(hsv.ToColor(), UpdateSource.Inputs);
    }

    private void HsvV_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_source.HasValue)
            return;

        //Trace.Write("HsvV_TextChanged");
        var v = ((ByteTextBox)sender).Value / 100f;
        var hsv = Hsv.From(SelectedColor);
        if (v == hsv.Value)
            return;

        hsv.Value = v;
        UpdateSelectedColor(hsv.ToColor(), UpdateSource.Inputs);
    }

    private void ArgbA_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_source.HasValue)
            return;

        //Trace.Write("ArgbA_TextChanged");
        var color = SelectedColor;
        UpdateSelectedColor(Color.FromArgb(((ByteTextBox)sender).Value, color.R, color.G, color.B), UpdateSource.Inputs);
    }

    private void ArgbR_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_source.HasValue)
            return;

        //Trace.Write("ArgbR_TextChanged");
        var color = SelectedColor;
        UpdateSelectedColor(Color.FromArgb(color.A, ((ByteTextBox)sender).Value, color.G, color.B), UpdateSource.Inputs);
    }

    private void ArgbG_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_source.HasValue)
            return;

        //Trace.Write("ArgbG_TextChanged");
        var color = SelectedColor;
        UpdateSelectedColor(Color.FromArgb(color.A, color.R, ((ByteTextBox)sender).Value, color.B), UpdateSource.Inputs);
    }

    private void ArgbB_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_source.HasValue)
            return;

        //Trace.Write("ArgbB_TextChanged");
        var color = SelectedColor;
        UpdateSelectedColor(Color.FromArgb(color.A, color.R, color.G, ((ByteTextBox)sender).Value), UpdateSource.Inputs);
    }
}
