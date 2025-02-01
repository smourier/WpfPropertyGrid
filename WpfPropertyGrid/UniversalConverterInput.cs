namespace WpfPropertyGrid;

public class UniversalConverterInput
{
    public virtual object? Value { get; set; }
    public virtual object? ValueToCompare { get; set; }
    public virtual object? MinimumValue { get; set; }
    public virtual object? MaximumValue { get; set; }
    public virtual object? ConverterParameter { get; set; }
    public virtual UniversalConverterOptions Options { get; set; } = UniversalConverterOptions.Convert;
    public virtual UniversalConverterOperator Operator { get; set; }
    public virtual bool Reverse { get; set; }
    public virtual StringComparison StringComparison { get; set; } = StringComparison.CurrentCultureIgnoreCase;

    public virtual UniversalConverterInput Clone() => new()
    {
        MaximumValue = MaximumValue,
        MinimumValue = MinimumValue,
        Operator = Operator,
        Options = Options,
        Value = Value,
        ValueToCompare = ValueToCompare,
        Reverse = Reverse,
        StringComparison = StringComparison
    };

    private Type? ValueToCompareToType(IFormatProvider? provider)
    {
        var type = Value as Type;
        if (type != null)
            return type;

        var name = ValueToCompareToString(provider, true);
        if (string.IsNullOrEmpty(name))
            return null;

        return TypeResolutionService.ResolveType(name);
    }

    private string? ValueToCompareToString(IFormatProvider? provider, bool forceConvert)
    {
        if (ValueToCompare == null)
            return null;

        var str = ValueToCompare as string;
        if (str == null)
        {
            if (forceConvert || (Options & UniversalConverterOptions.Convert) == UniversalConverterOptions.Convert)
            {
                str = ConversionService.ChangeType<string>(ValueToCompare, null, provider);
                str ??= string.Format(provider, "{0}", ValueToCompare);
            }
        }

        if ((Options & UniversalConverterOptions.Trim) == UniversalConverterOptions.Trim)
        {
            if (str != null)
            {
                str = str.Trim();
            }
        }

        if ((Options & UniversalConverterOptions.Nullify) == UniversalConverterOptions.Nullify)
        {
            if (str != null && str.Length == 0)
            {
                str = null;
            }
        }
        return str;
    }

    private string? ValueToString(IFormatProvider? provider)
    {
        if (Value == null)
            return null;

        if (Value is not string str)
        {
            str = ConversionService.ChangeType<string>(Value, null, provider) ?? string.Format(provider, "{0}", Value);
        }
        return str;
    }

    public virtual bool Matches(IFormatProvider? provider = null)
    {
        var ret = false;
        string? v;
        string? vtc;
        UniversalConverterInput clone;
        switch (Operator)
        {
            case UniversalConverterOperator.Equal:
                if (Value == null)
                {
                    if (ValueToCompare == null)
                    {
                        ret = true;
                        break;
                    }

                    // use string comparison (to compare "" to null or similar)
                    v = ValueToCompareToString(provider, false);
                    ret = v == null;
                    break; // false
                }

                if (Value.Equals(ValueToCompare))
                {
                    ret = true;
                    break;
                }

                v = Value as string;
                if (v != null)
                {
                    vtc = ValueToCompareToString(provider, true);
                    ret = string.Compare(v, vtc, StringComparison) == 0;
                    break;
                }

                if ((Options & UniversalConverterOptions.Convert) == UniversalConverterOptions.Convert)
                {
                    if (ConversionService.TryChangeType(ValueToCompare, Value.GetType(), provider, out object cvalue))
                    {
                        if (Value.Equals(cvalue))
                        {
                            ret = true;
                            break;
                        }

                        if (Value is string)
                        {
                            var sv = (string)cvalue;
                            if ((Options & UniversalConverterOptions.Trim) == UniversalConverterOptions.Trim)
                            {
                                sv = sv.Trim();
                            }

                            if ((Options & UniversalConverterOptions.Nullify) == UniversalConverterOptions.Nullify)
                            {
                                if (sv.Length == 0)
                                {
                                    sv = null;
                                }
                            }

                            if (Value.Equals(sv))
                            {
                                ret = true;
                                break;
                            }
                        }
                    }
                }
                break;

            case UniversalConverterOperator.NotEqual:
                clone = Clone();
                clone.Operator = UniversalConverterOperator.Equal;
                ret = clone.Matches(provider);
                break;

            case UniversalConverterOperator.Contains:
                v = ValueToString(provider);
                if (v == null)
                    break;

                vtc = ValueToCompareToString(provider, true);
                if (vtc == null)
                    break;

                ret = v.Contains(vtc, StringComparison);
                break;

            case UniversalConverterOperator.StartsWith:
                v = ValueToString(provider);
                if (v == null)
                    break;

                vtc = ValueToCompareToString(provider, true);
                if (vtc == null)
                    break;

                ret = v.StartsWith(vtc, StringComparison);
                break;

            case UniversalConverterOperator.EndsWith:
                v = ValueToString(provider);
                if (v == null)
                    break;

                vtc = ValueToCompareToString(provider, true);
                if (vtc == null)
                    break;

                ret = v.EndsWith(vtc, StringComparison);
                break;

            case UniversalConverterOperator.LesserThanOrEqual:
            case UniversalConverterOperator.LesserThan:
            case UniversalConverterOperator.GreaterThanOrEqual:
            case UniversalConverterOperator.GreaterThan:
                if (Value is not IComparable cv || ValueToCompare == null)
                    break;

                IComparable? cvtc;
                if (!Value.GetType().IsAssignableFrom(ValueToCompare.GetType()))
                {
                    cvtc = ConversionService.ChangeType(ValueToCompare, Value.GetType(), provider) as IComparable;
                }
                else
                {
                    cvtc = ValueToCompare as IComparable;
                }
                if (cvtc == null)
                    break;

                int comparison;
                v = Value as string;
                if (StringComparison != StringComparison.CurrentCulture) // not the default? use the special string comparer
                {
                    vtc = ValueToCompareToString(provider, true);
                    comparison = string.Compare(v, vtc, StringComparison);
                }
                else
                {
                    comparison = cv.CompareTo(cvtc);
                }

                if (comparison == 0)
                {
                    ret = Operator == UniversalConverterOperator.GreaterThanOrEqual || Operator == UniversalConverterOperator.LesserThanOrEqual;
                    break;
                }

                if (comparison < 0)
                {
                    ret = Operator == UniversalConverterOperator.LesserThan || Operator == UniversalConverterOperator.LesserThanOrEqual;
                    break;
                }

                ret = Operator == UniversalConverterOperator.GreaterThan || Operator == UniversalConverterOperator.GreaterThanOrEqual;
                break;

            case UniversalConverterOperator.Between:
                clone = Clone();
                clone.ValueToCompare = MinimumValue;
                clone.Operator = UniversalConverterOperator.GreaterThanOrEqual;
                if (!clone.Matches(provider))
                    break;

                clone = Clone();
                clone.ValueToCompare = MaximumValue;
                clone.Operator = UniversalConverterOperator.LesserThan;
                ret = clone.Matches(provider);
                break;

            case UniversalConverterOperator.IsType:
            case UniversalConverterOperator.IsOfType:
                var tvtc = ValueToCompareToType(provider);
                if (tvtc == null)
                    break;

                if (Value == null)
                {
                    if (tvtc.IsValueType)
                        break;

                    ret = (Options & UniversalConverterOptions.NullMatchesType) == UniversalConverterOptions.NullMatchesType;
                    break;
                }

                if (Operator == UniversalConverterOperator.IsType)
                {
                    ret = Value.GetType() == tvtc;
                }
                else
                {
                    ret = tvtc.IsAssignableFrom(Value.GetType());
                }
                break;
        }

        return Reverse ? !ret : ret;
    }
}
