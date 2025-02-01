namespace WpfPropertyGrid;

public class UniversalConverterCase
{
    public virtual object? Value { get; set; }
    public virtual object? ConvertedValue { get; set; }
    public virtual object? MinimumValue { get; set; }
    public virtual object? MaximumValue { get; set; }
    public virtual UniversalConverterOptions Options { get; set; }
    public virtual UniversalConverterOperator Operator { get; set; }
    public virtual StringComparison StringComparison { get; set; } = StringComparison.CurrentCultureIgnoreCase;
    public virtual bool Reverse { get; set; }

    public virtual bool Matches(object? value, object? parameter, IFormatProvider? culture)
    {
        var input = new UniversalConverterInput
        {
            MaximumValue = MaximumValue,
            MinimumValue = MinimumValue,
            Operator = Operator,
            Options = Options,
            Value = Value,
            ValueToCompare = value,
            Reverse = Reverse,
            StringComparison = StringComparison,
            ConverterParameter = parameter
        };
        return input.Matches(culture);
    }
}
