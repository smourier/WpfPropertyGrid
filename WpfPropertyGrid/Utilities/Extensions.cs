namespace WpfPropertyGrid.Utilities;

public static class Extensions
{
    private const string _hexaChars = "0123456789ABCDEF";

    public static string? ToHexa(byte[] bytes)
    {
        if (bytes == null)
            return null;

        return ToHexa(bytes, 0, bytes.Length);
    }

    public static string ToHexa(byte[] bytes, int offset, int count)
    {
        if (bytes == null)
            return string.Empty;

        if (offset < 0)
            throw new ArgumentException(null, nameof(offset));

        if (count < 0)
            throw new ArgumentException(null, nameof(count));

        if (offset >= bytes.Length)
            return string.Empty;

        count = Math.Min(count, bytes.Length - offset);

        var sb = new StringBuilder(count * 2);
        for (int i = offset; i < (offset + count); i++)
        {
            sb.Append(_hexaChars[bytes[i] / 16]);
            sb.Append(_hexaChars[bytes[i] % 16]);
        }
        return sb.ToString();
    }

    public static object EnumToObject(Type enumType, object value)
    {
        ArgumentNullException.ThrowIfNull(enumType);

        if (!enumType.IsEnum)
            throw new ArgumentException(null, nameof(enumType));

        ArgumentNullException.ThrowIfNull(value);

        var underlyingType = Enum.GetUnderlyingType(enumType);
        if (underlyingType == typeof(long))
            return Enum.ToObject(enumType, ConversionService.ChangeType<long>(value));

        if (underlyingType == typeof(ulong))
            return Enum.ToObject(enumType, ConversionService.ChangeType<ulong>(value));

        if (underlyingType == typeof(int))
            return Enum.ToObject(enumType, ConversionService.ChangeType<int>(value));

        if (underlyingType == typeof(uint))
            return Enum.ToObject(enumType, ConversionService.ChangeType<uint>(value));

        if (underlyingType == typeof(short))
            return Enum.ToObject(enumType, ConversionService.ChangeType<short>(value));

        if (underlyingType == typeof(ushort))
            return Enum.ToObject(enumType, ConversionService.ChangeType<ushort>(value));

        if (underlyingType == typeof(byte))
            return Enum.ToObject(enumType, ConversionService.ChangeType<byte>(value));

        if (underlyingType == typeof(sbyte))
            return Enum.ToObject(enumType, ConversionService.ChangeType<sbyte>(value));

        throw new ArgumentException(null, nameof(enumType));
    }

    public static string Format(object? obj, string? format, IFormatProvider? formatProvider)
    {
        if (obj == null)
            return string.Empty;

        if (string.IsNullOrEmpty(format))
            return obj.ToString() ?? string.Empty;

        if (format.StartsWith('*') ||
            format.StartsWith('#'))
        {
            char sep1 = ' ';
            char sep2 = ':';
            if (format.Length > 1)
            {
                sep1 = format[1];
            }
            if (format.Length > 2)
            {
                sep2 = format[2];
            }

            var sb = new StringBuilder();
            foreach (PropertyInfo pi in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!pi.CanRead)
                    continue;

                if (pi.GetIndexParameters().Length > 0)
                    continue;

                object? value;
                try
                {
                    value = pi.GetValue(obj, null);
                }
                catch
                {
                    continue;
                }
                if (sb.Length > 0)
                {
                    if (sep1 != ' ')
                    {
                        sb.Append(sep1);
                    }
                    sb.Append(' ');
                }

                if (format[0] == '#')
                {
                    sb.Append(DecamelizationService.Decamelize(pi.Name));
                }
                else
                {
                    sb.Append(pi.Name);
                }
                sb.Append(sep2);
                sb.Append(ConversionService.ChangeType(value, string.Format("{0}", value), formatProvider));
            }
            return sb.ToString();
        }

        if (format.StartsWith("Item[", StringComparison.CurrentCultureIgnoreCase))
        {
            string enumExpression;
            var exprPos = format.IndexOf(']', 5);
            if (exprPos < 0)
            {
                enumExpression = string.Empty;
            }
            else
            {
                enumExpression = format[5..exprPos].Trim();
            }

            if (obj is IEnumerable enumerable)
            {
                format = format[(6 + enumExpression.Length)..];
                string? expression;
                string separator;
                if (format.Length == 0)
                {
                    expression = null;
                    separator = ",";
                }
                else
                {
                    var pos = format.IndexOf(',');
                    if (pos <= 0)
                    {
                        separator = ",";
                        expression = format[1..];
                    }
                    else
                    {
                        separator = format[(pos + 1)..];
                        expression = format[1..pos];
                    }
                }
                return ConcatenateCollection(enumerable, expression ?? string.Empty, separator, formatProvider) ?? string.Empty;
            }
        }
        else if (format.Contains(','))
        {
            var sb = new StringBuilder();
            foreach (var propName in format.Split([','], StringSplitOptions.RemoveEmptyEntries))
            {
                var pi = obj.GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.Public);
                if ((pi == null) || (!pi.CanRead))
                    continue;

                if (pi.GetIndexParameters().Length > 0)
                    continue;

                object? value;
                try
                {
                    value = pi.GetValue(obj, null);
                }
                catch
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append(' ');
                }
                sb.Append(pi.Name);
                sb.Append(':');
                sb.AppendFormat(formatProvider, "{0}", value);
            }
            return sb.ToString();
        }

        int pos2 = format.IndexOf(':');
        if (pos2 > 0)
        {
            var inner = DataBindingEvaluator.Eval(obj, format[..pos2], false);
            if (inner == null)
                return string.Empty;

            return string.Format(formatProvider, "{0:" + format[(pos2 + 1)..] + "}", inner);
        }
        return DataBindingEvaluator.EvalFormat(obj, format, formatProvider, null, false);
    }

    public static string? ConcatenateCollection(IEnumerable collection, string expression, string? separator, IFormatProvider? formatProvider = null)
    {
        if (collection == null)
            return null;

        var sb = new StringBuilder();
        var i = 0;
        foreach (var obj in collection)
        {
            if (i > 0)
            {
                sb.Append(separator);
            }
            else
            {
                i++;
            }

            if (obj != null)
            {
                var e = DataBindingEvaluator.EvalFormat(obj, expression, formatProvider, null, false);
                if (e != null)
                {
                    sb.Append(e);
                }
            }
        }
        return sb.ToString();
    }

    public static Type GetElementType(Type collectionType)
    {
        ArgumentNullException.ThrowIfNull(collectionType);

        foreach (var iface in collectionType.GetInterfaces())
        {
            if (!iface.IsGenericType)
                continue;

            if (iface.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                return iface.GetGenericArguments()[1];

            if (iface.GetGenericTypeDefinition() == typeof(IList<>))
                return iface.GetGenericArguments()[0];

            if (iface.GetGenericTypeDefinition() == typeof(ICollection<>))
                return iface.GetGenericArguments()[0];

            if (iface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return iface.GetGenericArguments()[0];
        }
        return typeof(object);
    }

    public static int GetEnumMaxPower(Type enumType)
    {
        ArgumentNullException.ThrowIfNull(enumType);

        if (!enumType.IsEnum)
            throw new ArgumentException(null, nameof(enumType));

        var type = Enum.GetUnderlyingType(enumType);
        return GetEnumUnderlyingTypeMaxPower(type);
    }

    public static int GetEnumUnderlyingTypeMaxPower(Type underlyingType)
    {
        ArgumentNullException.ThrowIfNull(underlyingType);

        if (underlyingType == typeof(long) || underlyingType == typeof(ulong))
            return 64;

        if (underlyingType == typeof(int) || underlyingType == typeof(uint))
            return 32;

        if (underlyingType == typeof(short) || underlyingType == typeof(ushort))
            return 16;

        if (underlyingType == typeof(byte) || underlyingType == typeof(sbyte))
            return 8;

        throw new ArgumentException(null, nameof(underlyingType));
    }

    public static ulong EnumToUInt64(object value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var typeCode = Convert.GetTypeCode(value);
        return typeCode switch
        {
            TypeCode.SByte or TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 => (ulong)Convert.ToInt64(value, CultureInfo.InvariantCulture),
            TypeCode.Byte or TypeCode.UInt16 or TypeCode.UInt32 or TypeCode.UInt64 => Convert.ToUInt64(value, CultureInfo.InvariantCulture),
            _ => ConversionService.ChangeType<ulong>(value),
        };
    }

    public static bool IsFlagsEnum(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (!type.IsEnum)
            return false;

        return type.IsDefined(typeof(FlagsAttribute), true);
    }

    public static List<T> SplitToList<T>(this string? thisString, params char[] separators)
    {
        var list = new List<T>();
        if (thisString != null)
        {
            foreach (var s in thisString.Split(separators))
            {
                var item = ConversionService.ChangeType<T>(s);
                if (item != null)
                {
                    list.Add(item);
                }
            }
        }
        return list;
    }

    public static string? Nullify(this string? thisString, bool trim = true)
    {
        if (string.IsNullOrWhiteSpace(thisString))
            return null;

        return trim ? thisString.Trim() : thisString;
    }

    public static bool EqualsIgnoreCase(this string? thisString, string? text, bool trim = true)
    {
        if (trim)
        {
            thisString = Nullify(thisString, true);
            text = Nullify(text, true);
        }

        if (thisString == null)
            return text == null;

        if (text == null)
            return false;

        if (thisString.Length != text.Length)
            return false;

        return string.Compare(thisString, text, StringComparison.OrdinalIgnoreCase) == 0;
    }

    public static IEnumerable<DependencyObject> EnumerateVisualChildren(this DependencyObject obj, bool recursive = true, bool sameLevelFirst = true)
    {
        if (obj == null)
            yield break;

        if (sameLevelFirst)
        {
            var count = VisualTreeHelper.GetChildrenCount(obj);
            var list = new List<DependencyObject>(count);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child == null)
                    continue;

                yield return child;
                if (recursive)
                {
                    list.Add(child);
                }
            }

            foreach (var child in list)
            {
                foreach (var grandChild in child.EnumerateVisualChildren(recursive, true))
                {
                    yield return grandChild;
                }
            }
        }
        else
        {
            var count = VisualTreeHelper.GetChildrenCount(obj);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child == null)
                    continue;

                yield return child;
                if (recursive)
                {
                    foreach (var dp in child.EnumerateVisualChildren(true, false))
                    {
                        yield return dp;
                    }
                }
            }
        }
    }

    public static T? FindVisualChild<T>(this DependencyObject obj, Func<T, bool> where) where T : FrameworkElement
    {
        ArgumentNullException.ThrowIfNull(where);

        foreach (var item in obj.EnumerateVisualChildren(true, true).OfType<T>())
        {
            if (where(item))
                return item;
        }
        return null;
    }

    public static T? FindVisualChild<T>(this DependencyObject obj, string name) where T : FrameworkElement
    {
        foreach (var item in obj.EnumerateVisualChildren(true, true).OfType<T>())
        {
            if (name == null)
                return item;

            if (item.Name == name)
                return item;
        }
        return null;
    }

    public static IEnumerable<DependencyProperty> EnumerateMarkupDependencyProperties(object element)
    {
        if (element != null)
        {
            MarkupObject markupObject = MarkupWriter.GetMarkupObjectFor(element);
            if (markupObject != null)
            {
                foreach (MarkupProperty mp in markupObject.Properties)
                {
                    if (mp.DependencyProperty != null)
                        yield return mp.DependencyProperty;
                }
            }
        }
    }

    public static IEnumerable<DependencyProperty> EnumerateMarkupAttachedProperties(object element)
    {
        if (element != null)
        {
            var markupObject = MarkupWriter.GetMarkupObjectFor(element);
            if (markupObject != null)
            {
                foreach (var mp in markupObject.Properties)
                {
                    if (mp.IsAttached)
                        yield return mp.DependencyProperty;
                }
            }
        }
    }

    public static T? GetVisualSelfOrParent<T>(this DependencyObject source) where T : DependencyObject
    {
        if (source == null)
            return default;

        if (source is T t)
            return t;

        if (source is not Visual && source is not Visual3D)
            return default;

        return VisualTreeHelper.GetParent(source).GetVisualSelfOrParent<T>();
    }

    public static T? FindFocusableVisualChild<T>(this DependencyObject obj, string name) where T : FrameworkElement
    {
        foreach (var item in obj.EnumerateVisualChildren(true, true).OfType<T>())
        {
            if (item.Focusable && (item.Name == name || name == null))
                return item;
        }
        return null;
    }

    public static IEnumerable<T> GetChildren<T>(this DependencyObject obj)
    {
        if (obj == null)
            yield break;

        foreach (var item in LogicalTreeHelper.GetChildren(obj))
        {
            if (item == null)
                continue;

            if (item is T t)
                yield return t;

            if (item is DependencyObject dep)
            {
                foreach (T child in dep.GetChildren<T>())
                {
                    yield return child;
                }
            }
        }
    }

    public static IEnumerable<Expander> EnumerateExpanders(this DataGrid grid)
    {
        ArgumentNullException.ThrowIfNull(grid);

        foreach (var item in grid.EnumerateVisualChildren(true, true).OfType<GroupItem>())
        {
            var expander = item.FindVisualChild<Expander>(ex => true);
            if (expander != null)
                yield return expander;
        }
    }

    public static Expander? GetExpander(this DataGrid grid, object groupName)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(groupName);

        foreach (var expander in EnumerateExpanders(grid))
        {
            if (expander.DataContext is CollectionViewGroup group && groupName.Equals(group.Name))
                return expander;
        }
        return null;
    }

    public static T? GetSelfOrParent<T>(this FrameworkElement? source) where T : FrameworkElement
    {
        while (true)
        {
            if (source == null)
                return default;

            if (source is T t)
                return t;

            source = source.Parent as FrameworkElement;
        }
    }

    public static string? GetAllMessages(this Exception? exception) => GetAllMessages(exception, Environment.NewLine);
    public static string? GetAllMessages(this Exception? exception, string? separator = null)
    {
        if (exception == null)
            return null;

        var sb = new StringBuilder();
        AppendMessages(sb, exception, separator);
        return sb.ToString().Replace("..", ".");
    }

    private static void AppendMessages(StringBuilder sb, Exception? e, string? separator)
    {
        if (e == null)
            return;

        separator ??= Environment.NewLine;
        if (e is not TargetInvocationException)
        {
            if (sb.Length > 0)
            {
                sb.Append(separator);
            }
            sb.Append(e.Message);
        }
        AppendMessages(sb, e.InnerException, separator);
    }

    public static bool IsNullable(this Type type)
    {
        if (type == null)
            return false;

        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
}
