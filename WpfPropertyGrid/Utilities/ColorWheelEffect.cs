using System.Windows.Media.Effects;

namespace WpfPropertyGrid.Utilities;

public class ColorWheelEffect : ShaderEffect
{
    public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty(nameof(Input), typeof(ColorWheelEffect), 0);

    public static readonly DependencyProperty CenterProperty = DependencyProperty.Register(nameof(Center), typeof(Point), typeof(ColorWheelEffect),
        new PropertyMetadata(new Point(0.5, 0.5), PixelShaderConstantCallback(0)));

    public ColorWheelEffect()
    {
        PixelShader = new PixelShader() { UriSource = new Uri("/WpfPropertyGrid;component/Resources/ColorWheel.ps", UriKind.Relative) };
        UpdateShaderValue(InputProperty);
        UpdateShaderValue(CenterProperty);
    }

    public Brush Input { get { return (Brush)GetValue(InputProperty); } set { SetValue(InputProperty, value); } }
    public Point Center { get { return (Point)GetValue(CenterProperty); } set { SetValue(CenterProperty, value); } }
}
