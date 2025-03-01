namespace WpfPropertyGrid.Utilities;

public struct Hsv(float hue, float saturation, float value) : IEquatable<Hsv>
{
    public float Hue = hue;
    public float Saturation = saturation;
    public float Value = value;
    public readonly Color Color => ToColor();

    public readonly int ToArgb()
    {
        var color = Color;
        return (int)(((uint)color.A << 24) | (uint)(color.R << 16) | (uint)(color.G << 8) | color.B);
    }

    public readonly Color ToColor(float a = 1)
    {
        var hue = Hue * 360;
        var saturation = Saturation;
        var value = Value;

        while (hue >= 360)
        {
            hue -= 360;
        }

        while (hue < 0)
        {
            hue += 360;
        }

        saturation = saturation < 0 ? 0 : saturation;
        saturation = saturation > 1 ? 1 : saturation;

        value = value < 0 ? 0 : value;
        value = value > 1 ? 1 : value;

        var chroma = saturation * value;
        var min = value - chroma;

        if (chroma == 0)
            return Color.FromScRgb(a, min, min, min);

        var sextant = (int)(hue / 60);
        var intermediateColorPercentage = hue / 60 - sextant;
        var max = chroma + min;

        var r = 0f;
        var g = 0f;
        var b = 0f;

        switch (sextant)
        {
            case 0:
                r = max;
                g = min + chroma * intermediateColorPercentage;
                b = min;
                break;

            case 1:
                r = min + chroma * (1 - intermediateColorPercentage);
                g = max;
                b = min;
                break;

            case 2:
                r = min;
                g = max;
                b = min + chroma * intermediateColorPercentage;
                break;

            case 3:
                r = min;
                g = min + chroma * (1 - intermediateColorPercentage);
                b = max;
                break;

            case 4:
                r = min + chroma * intermediateColorPercentage;
                g = min;
                b = max;
                break;

            case 5:
                r = max;
                g = min;
                b = min + chroma * (1 - intermediateColorPercentage);
                break;
        }

        return Color.FromScRgb(a, r, g, b);
    }

    public override readonly int GetHashCode() => Hue.GetHashCode() ^ Saturation.GetHashCode() ^ Value.GetHashCode();
    public override readonly bool Equals(object? other) => other is Hsv hsv && Equals(hsv);
    public readonly bool Equals(Hsv other) => Value == other.Value && Hue == other.Hue && Saturation == other.Saturation;
    public override readonly string ToString() => "H:" + Hue + " S:" + Saturation + " V:" + Value;
    public static implicit operator Color(Hsv result) => result.ToColor();
    public static implicit operator Hsv(Color color) => From(color);
    public static bool operator ==(Hsv x, Hsv y) => x.Equals(y);
    public static bool operator !=(Hsv x, Hsv y) => !x.Equals(y);

    public static Hsv From(Color color) => new(GetHue(color.ScR, color.ScG, color.ScB), GetSaturation(color.ScR, color.ScG, color.ScB), GetValue(color.ScR, color.ScG, color.ScB));

    public static float GetHue(float r, float g, float b)
    {
        if (r == g && g == b)
            return 0;

        var max = r;
        var min = r;

        if (g > max) max = g;
        if (b > max) max = b;
        if (g < min) min = g;
        if (b < min) min = b;

        var delta = max - min;
        var hue = 0f;
        if (r == max)
        {
            hue = (g - b) / delta;
        }
        else if (g == max)
        {
            hue = 2 + (b - r) / delta;
        }
        else if (b == max)
        {
            hue = 4 + (r - g) / delta;
        }

        hue *= 60;

        if (hue < 0)
        {
            hue += 360;
        }
        return hue / 360;
    }

    public static float GetValue(float r, float g, float b) => r >= g ? (r >= b ? r : b) : (g >= b ? g : b);
    public static float GetSaturation(float r, float g, float b)
    {
        var min = r <= g ? (r <= b ? r : b) : (g <= b ? g : b);
        var value = GetValue(r, g, b);
        var chroma = value - min;
        if (chroma == 0)
            return 0;

        return chroma / value;
    }
}
