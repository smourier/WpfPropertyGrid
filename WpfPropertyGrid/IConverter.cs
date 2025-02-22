namespace WpfPropertyGrid;

public interface IConverter
{
    bool TryConvert(object? input, Type conversionType, IFormatProvider? provider, out object? value);
}
