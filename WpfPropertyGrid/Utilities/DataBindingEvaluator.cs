namespace WpfPropertyGrid.Utilities;

public static class DataBindingEvaluator
{
    private static readonly char[] expressionPartSeparator = ['.'];
    private static readonly char[] indexExprEndChars = [']', ')'];
    private static readonly char[] indexExprStartChars = ['[', '('];

    public static string Eval(object? container, string expression, string format, bool throwOnError = true) => Eval(container, expression, null, format, throwOnError);
    public static string Eval(object? container, string expression, IFormatProvider? provider, string format, bool throwOnError = true)
    {
        format ??= "{0}";
        if (provider == null)
            return string.Format(format, Eval(container, expression, throwOnError));

        return string.Format(provider, format, Eval(container, expression));
    }

    public static object? Eval(object? container, string expression, bool throwOnError = true)
    {
        ArgumentNullException.ThrowIfNull(expression);
        expression = expression.Nullify()!;
        if (expression == null)
            throw new ArgumentException(null, nameof(expression));

        if (container == null)
            return null;

        var expressionParts = expression.Split(expressionPartSeparator);
        return Eval(container, expressionParts, throwOnError);
    }

    private static object? Eval(object? container, string[] expressionParts, bool throwOnError)
    {
        var propertyValue = container;
        for (var i = 0; (i < expressionParts.Length) && (propertyValue != null); i++)
        {
            var propName = expressionParts[i];
            if (propName.IndexOfAny(indexExprStartChars) < 0)
            {
                propertyValue = GetPropertyValue(propertyValue, propName, throwOnError);
            }
            else
            {
                propertyValue = GetIndexedPropertyValue(propertyValue, propName, throwOnError);
            }
        }
        return propertyValue;
    }

    public static object? GetPropertyValue(object? container, string? propName, bool throwOnError = true)
    {
        ArgumentNullException.ThrowIfNull(container);

        propName = propName.Nullify();
        if (propName == null)
            throw new ArgumentException(null, nameof(propName));

        var descriptor = TypeDescriptor.GetProperties(container).Find(propName, true);
        if (descriptor == null)
        {
            if (throwOnError)
                throw new ArgumentException(string.Format(@"DataBindingEvaluator: '{0}' does not contain a property with the name '{1}'.", container.GetType().FullName, propName), nameof(propName));

            return null;
        }
        return descriptor.GetValue(container);
    }

    public static object? GetIndexedPropertyValue(object? container, string? expression, bool throwOnError = true)
    {
        ArgumentNullException.ThrowIfNull(container);

        expression = expression.Nullify();
        if (expression == null)
            throw new ArgumentException(null, nameof(expression));

        var numberIndex = false;
        var idx1 = expression.IndexOfAny(indexExprStartChars);
        var idx2 = expression.IndexOfAny(indexExprEndChars, idx1 + 1);
        if (idx1 < 0 || idx2 < 0 || idx2 == (idx1 + 1))
        {
            if (throwOnError)
                throw new ArgumentException(string.Format(@"DataBindingEvaluator: '{0}' is not a valid indexed expression.", new object[] { expression }));

            return null;
        }

        string? propName = null;
        object? index = null;
        var s = expression.Substring(idx1 + 1, (idx2 - idx1) - 1).Trim();
        if (idx1 != 0)
        {
            propName = expression[..idx1];
        }

        if (s.Length != 0)
        {
            if ((s[0] == '"' && s[^1] == '"') || (s[0] == '\'' && s[^1] == '\''))
            {
                index = s[1..^1];
            }
            else if (char.IsDigit(s[0]))
            {
                numberIndex = int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int nums);
                if (numberIndex)
                {
                    index = nums;
                }
                else
                {
                    index = s;
                }
            }
            else
            {
                index = s;
            }
        }
        if (index == null)
        {
            if (throwOnError)
                throw new ArgumentException(string.Format(@"DataBindingEvaluator: '{0}' is not a valid indexed expression.", new object[] { expression }));

            return null;
        }

        object? propertyValue;
        if (!string.IsNullOrEmpty(propName))
        {
            propertyValue = GetPropertyValue(container, propName);
        }
        else
        {
            propertyValue = container;
        }

        if (propertyValue == null)
            return null;

        if (propertyValue is Array array && numberIndex)
            return array.GetValue((int)index);

        if ((propertyValue is IList list) && numberIndex)
            return list[(int)index];

        var info = propertyValue.GetType().GetProperty("Item", BindingFlags.Public | BindingFlags.Instance, null, null, [index.GetType()], null);
        if (info == null)
        {
            if (throwOnError)
                throw new ArgumentException(string.Format(@"DataBindingEvaluator: '{0}' does not allow indexed access.", [propertyValue.GetType().FullName]));

            return null;
        }
        return info.GetValue(propertyValue, [index]);
    }
}
