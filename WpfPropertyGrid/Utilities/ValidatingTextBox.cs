namespace WpfPropertyGrid.Utilities;

public class ValidatingTextBox : TextBox
{
    private bool _inEvents;
    private string? _textBefore;
    private string? _initalText;
    private int _selectionStart;
    private int _selectionLength;

    // validates entered text at char level, but globally
    public event EventHandler<ValidateTextEventArgs>? PreValidateText;

    // validates before losing focus
    public event EventHandler<ValidateTextEventArgs>? ValidateText;

    protected virtual void OnPreValidateText(object sender, ValidateTextEventArgs e) => PreValidateText?.Invoke(this, e);
    protected virtual void OnValidateText(object sender, ValidateTextEventArgs e) => ValidateText?.Invoke(this, e);

    protected override void OnGotFocus(RoutedEventArgs e)
    {
        base.OnGotFocus(e);
        _initalText = Text;
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        var ev = new ValidateTextEventArgs(Text);
        OnValidateText(this, ev);
        if (ev.Cancel && Text != _initalText)
        {
            Text = _initalText;
        }
        else if (ev.ReplacementText != null && Text != ev.ReplacementText)
        {
            Text = ev.ReplacementText;
        }
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (_inEvents)
            return;

        _selectionStart = SelectionStart;
        _selectionLength = SelectionLength;
        _textBefore = Text;
    }

    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        if (_inEvents)
            return;

        _inEvents = true;
        try
        {
            var ev = new ValidateTextEventArgs(Text);
            OnPreValidateText(this, ev);
            if (ev.Cancel)
            {
                Text = _textBefore;
                SelectionStart = _selectionStart;
                SelectionLength = _selectionLength;
            }
            else if (ev.ReplacementText != null && ev.ReplacementText != Text)
            {
                Text = ev.ReplacementText;
            }
            base.OnTextChanged(e);
        }
        finally
        {
            _inEvents = false;
        }
    }
}

public abstract class NumberTextBox<T> : ValidatingTextBox where T : struct, INumberBase<T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
{
    protected virtual bool IsFloatingPoint => false;
    protected virtual bool IsEmptyValid => true;
    protected virtual bool IsSigned
    {
        get
        {
            if (MinValue.HasValue && MinValue.Value >= T.Zero)
                return false;

            return true;
        }
    }

    public T? MinValue { get; set; }
    public T? MaxValue { get; set; }
    public T Value
    {
        get
        {
            var norm = Text.Replace(',', '.');
            T.TryParse(norm, CultureInfo.InvariantCulture, out var value);
            return value;
        }
    }

    protected virtual bool IsValidDigit(char c) => char.IsAsciiDigit(c);
    protected override void OnValidateText(object sender, ValidateTextEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.Text))
        {
            var norm = e.Text.Replace(',', '.');
            if (T.TryParse(norm, CultureInfo.InvariantCulture, out var value))
            {
                if (MinValue.HasValue && value < MinValue.Value)
                {
                    value = MinValue.Value;
                }

                if (MaxValue.HasValue && value >= MaxValue.Value)
                {
                    value = MaxValue.Value;
                }

                e.ReplacementText = value.ToString();
                return;
            }

            if (IsFloatingPoint)
            {
                if (typeof(T) != typeof(double) && double.TryParse(norm, CultureInfo.InvariantCulture, out var dbl))
                {
                    e.ReplacementText = dbl > 0 ? (MaxValue ?? T.MaxValue).ToString() : (MinValue ?? T.MinValue).ToString();
                    return;
                }
            }
            else
            {
                if (IsSigned)
                {
                    if (typeof(T) != typeof(long) && long.TryParse(norm, CultureInfo.InvariantCulture, out var lg))
                    {
                        e.ReplacementText = lg > 0 ? (MaxValue ?? T.MaxValue).ToString() : (MinValue ?? T.MinValue).ToString();
                        return;
                    }
                }
                else
                {
                    if (typeof(T) != typeof(ulong) && ulong.TryParse(norm, CultureInfo.InvariantCulture, out var ulg))
                    {
                        e.ReplacementText = ulg > 0 ? (MaxValue ?? T.MaxValue).ToString() : (MinValue ?? T.MinValue).ToString();
                        return;
                    }
                }
            }
        }
        e.Cancel = true;
    }

    protected override void OnPreValidateText(object sender, ValidateTextEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.Text))
        {
            e.Cancel = !IsEmptyValid;
            return;
        }

        var dashes = 0;
        var floatingPoints = 0;
        for (var i = 0; i < e.Text.Length; i++)
        {
            var c = e.Text[i];
            if (c == '-' && IsSigned)
            {
                dashes++;
                if (dashes > 1)
                {
                    e.Cancel = true;
                    return;
                }
                continue;
            }

            // note: we don't support scientific notation
            if ((c == '.' || c == ',') && IsFloatingPoint)
            {
                floatingPoints++;
                if (floatingPoints > 1)
                {
                    e.Cancel = true;
                    return;
                }
                continue;
            }

            if (!IsValidDigit(c))
            {
                e.Cancel = true;
                return;
            }
        }
    }
}

public abstract class SignedIntegerTextBox<T> : NumberTextBox<T> where T : struct, ISignedNumber<T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
{
}

public abstract class UnsignedIntegerTextBox<T> : NumberTextBox<T> where T : struct, IUnsignedNumber<T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
{
    protected override bool IsSigned => false;
}

public abstract class FloatingPointNumberTextBox<T> : NumberTextBox<T> where T : struct, IFloatingPoint<T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
{
    protected override bool IsFloatingPoint => true;
}

public class SByteTextBox : SignedIntegerTextBox<sbyte>
{
}

public class ByteTextBox : UnsignedIntegerTextBox<byte>
{
}

public class Int16TextBox : SignedIntegerTextBox<short>
{
}

public class Int32TextBox : SignedIntegerTextBox<int>
{
}

public class Int64TextBox : SignedIntegerTextBox<long>
{
}

public class UInt16TextBox : UnsignedIntegerTextBox<ushort>
{
}

public class UInt32TextBox : UnsignedIntegerTextBox<uint>
{
}

public class UInt64TextBox : UnsignedIntegerTextBox<ulong>
{
}

public class DoubleTextBox : FloatingPointNumberTextBox<double>
{
}

public class SingleTextBox : FloatingPointNumberTextBox<float>
{
}

public class DecimalTextBox : FloatingPointNumberTextBox<decimal>
{
}