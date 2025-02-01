﻿namespace WpfPropertyGrid;

public interface IConverter
{
    bool TryChangeType(object? input, Type conversionType, IFormatProvider? provider, out object? value);
}
