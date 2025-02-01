﻿namespace WpfPropertyGrid;

public class BaseDecamelizer : IDecamelizer
{
    public virtual string? Decamelize(string? text, DecamelizeOptions? options)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        options ??= new DecamelizeOptions();

        var sb = new StringBuilder(text.Length);

        // 0=lower, 1=upper, 2=special char
        var lastCategory = CharUnicodeInfo.GetUnicodeCategory(text[0]);
        var prevCategory = lastCategory;
        if (lastCategory == UnicodeCategory.UppercaseLetter)
        {
            lastCategory = UnicodeCategory.LowercaseLetter;
        }

        var i = 0;
        var firstIsStillUnderscore = text[0] == '_';
        if (((options.TextOptions & DecamelizeTextOptions.UnescapeUnicode) == DecamelizeTextOptions.UnescapeUnicode) && CanUnicodeEscape(text, 0))
        {
            sb.Append(GetUnicodeEscape(text, ref i));
        }
        else if (((options.TextOptions & DecamelizeTextOptions.UnescapeHexadecimal) == DecamelizeTextOptions.UnescapeHexadecimal) && CanHexadecimalEscape(text, 0))
        {
            sb.Append(GetHexadecimalEscape(text, ref i));
        }
        else
        {
            if ((options.TextOptions & DecamelizeTextOptions.ForceFirstUpper) == DecamelizeTextOptions.ForceFirstUpper)
            {
                sb.Append(char.ToUpper(text[0]));
            }
            else
            {
                sb.Append(text[0]);
            }
        }

        var separated = false;
        var keepFormat = (options.TextOptions & DecamelizeTextOptions.KeepFormattingIndices) == DecamelizeTextOptions.KeepFormattingIndices;
        for (i++; i < text.Length; i++)
        {
            var c = text[i];
            if (((options.TextOptions & DecamelizeTextOptions.UnescapeUnicode) == DecamelizeTextOptions.UnescapeUnicode) && CanUnicodeEscape(text, i))
            {
                sb.Append(GetUnicodeEscape(text, ref i));
                separated = true;
            }
            else if (((options.TextOptions & DecamelizeTextOptions.UnescapeHexadecimal) == DecamelizeTextOptions.UnescapeHexadecimal) && CanHexadecimalEscape(text, i))
            {
                sb.Append(GetHexadecimalEscape(text, ref i));
                separated = true;
            }
            else if (c == '_')
            {
                if ((!firstIsStillUnderscore) || ((options.TextOptions & DecamelizeTextOptions.KeepFirstUnderscores) != DecamelizeTextOptions.KeepFirstUnderscores))
                {
                    sb.Append(' ');
                    separated = true;
                }
                else
                {
                    sb.Append(c);
                }
            }
            else
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                switch (category)
                {
                    case UnicodeCategory.ClosePunctuation:
                    case UnicodeCategory.ConnectorPunctuation:
                    case UnicodeCategory.DashPunctuation:
                    case UnicodeCategory.EnclosingMark:
                    case UnicodeCategory.FinalQuotePunctuation:
                    case UnicodeCategory.Format:
                    case UnicodeCategory.InitialQuotePunctuation:
                    case UnicodeCategory.LineSeparator:
                    case UnicodeCategory.OpenPunctuation:
                    case UnicodeCategory.OtherPunctuation:
                    case UnicodeCategory.ParagraphSeparator:
                    case UnicodeCategory.SpaceSeparator:
                    case UnicodeCategory.SpacingCombiningMark:
                        if (keepFormat && (c == '{'))
                        {
                            while (c != '}')
                            {
                                c = text[i++];
                                sb.Append(c);
                            }
                            i--;
                            separated = true;
                            break;
                        }

                        if ((options.TextOptions & DecamelizeTextOptions.ForceRestLower) == DecamelizeTextOptions.ForceRestLower)
                        {
                            sb.Append(char.ToLower(c));
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        sb.Append(' ');
                        separated = true;
                        break;

                    case UnicodeCategory.LetterNumber:
                    case UnicodeCategory.DecimalDigitNumber:
                    case UnicodeCategory.OtherNumber:

                    case UnicodeCategory.CurrencySymbol:
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.MathSymbol:
                    case UnicodeCategory.ModifierLetter:
                    case UnicodeCategory.ModifierSymbol:
                    case UnicodeCategory.NonSpacingMark:
                    case UnicodeCategory.OtherLetter:
                    case UnicodeCategory.OtherNotAssigned:
                    case UnicodeCategory.Control:
                    case UnicodeCategory.OtherSymbol:
                    case UnicodeCategory.Surrogate:
                    case UnicodeCategory.PrivateUse:
                    case UnicodeCategory.TitlecaseLetter:
                    case UnicodeCategory.UppercaseLetter:
                        if ((category != lastCategory) && (c != ' ') && IsNewCategory(category, options))
                        {
                            if ((!separated) && (prevCategory != UnicodeCategory.UppercaseLetter) &&
                                ((!firstIsStillUnderscore) || ((options.TextOptions & DecamelizeTextOptions.KeepFirstUnderscores) != DecamelizeTextOptions.KeepFirstUnderscores)))
                            {
                                sb.Append(' ');
                            }

                            if ((options.TextOptions & DecamelizeTextOptions.ForceRestLower) != 0)
                            {
                                sb.Append(char.ToLower(c));
                            }
                            else
                            {
                                sb.Append(char.ToUpper(c));
                            }

                            char upper = char.ToUpper(c);
                            category = CharUnicodeInfo.GetUnicodeCategory(upper);
                            lastCategory = category == UnicodeCategory.UppercaseLetter ? UnicodeCategory.LowercaseLetter : category;
                        }
                        else
                        {
                            if ((options.TextOptions & DecamelizeTextOptions.ForceRestLower) == DecamelizeTextOptions.ForceRestLower)
                            {
                                sb.Append(char.ToLower(c));
                            }
                            else
                            {
                                sb.Append(c);
                            }
                        }
                        separated = false;
                        break;
                }
                firstIsStillUnderscore = firstIsStillUnderscore && (c == '_');
                prevCategory = category;
            }
        }

        if ((options.TextOptions & DecamelizeTextOptions.ReplaceSpacesByUnderscore) == DecamelizeTextOptions.ReplaceSpacesByUnderscore)
            return sb.Replace(' ', '_').ToString();

        if ((options.TextOptions & DecamelizeTextOptions.ReplaceSpacesByMinus) == DecamelizeTextOptions.ReplaceSpacesByMinus)
            return sb.Replace(' ', '-').ToString();

        if ((options.TextOptions & DecamelizeTextOptions.ReplaceSpacesByDot) == DecamelizeTextOptions.ReplaceSpacesByDot)
            return sb.Replace(' ', '.').ToString();

        return sb.ToString();
    }

    // format is _xXXXX_
    private static bool CanHexadecimalEscape(string text, int i) => (i + 6) < text.Length && text[i] == '_' && text[i + 1] == 'x' && text[i + 6] == '_' &&
            IsHexNumber(text[i + 2]) &&
            IsHexNumber(text[i + 3]) &&
            IsHexNumber(text[i + 4]) &&
            IsHexNumber(text[i + 5]);

    private static bool IsHexNumber(char c) =>
        // note: we don't want to use Char.IsDigit nor Char.IsNumber
        (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');

    private static char GetHexadecimalEscape(string text, ref int i)
    {
        var s = text[i + 2].ToString(CultureInfo.InvariantCulture);
        s += text[i + 3].ToString(CultureInfo.InvariantCulture);
        s += text[i + 4].ToString(CultureInfo.InvariantCulture);
        s += text[i + 5].ToString(CultureInfo.InvariantCulture);
        i += 6;
        return (char)int.Parse(s, NumberStyles.HexNumber);
    }

    // format is \uXXXX
    private static bool CanUnicodeEscape(string text, int i) => (i + 5) < text.Length &&
            text[i] == '\\' &&
            text[i + 1] == 'u' &&
            IsPureNumber(text[i + 2]) &&
            IsPureNumber(text[i + 3]) &&
            IsPureNumber(text[i + 4]) &&
            IsPureNumber(text[i + 5]);

    private static char GetUnicodeEscape(string text, ref int i)
    {
        var s = text[i + 2].ToString(CultureInfo.InvariantCulture);
        s += text[i + 3].ToString(CultureInfo.InvariantCulture);
        s += text[i + 4].ToString(CultureInfo.InvariantCulture);
        s += text[i + 5].ToString(CultureInfo.InvariantCulture);
        i += 5;
        return (char)int.Parse(s);
    }

    private static bool IsPureNumber(char c) =>
        // note: we don't want to use Char.IsDigit nor Char.IsNumber
        c >= '0' && c <= '9';

    private static bool IsNewCategory(UnicodeCategory category, DecamelizeOptions options)
    {
        if ((options.TextOptions & DecamelizeTextOptions.DontDecamelizeNumbers) == DecamelizeTextOptions.DontDecamelizeNumbers)
        {
            if (category == UnicodeCategory.LetterNumber ||
                category == UnicodeCategory.DecimalDigitNumber ||
                category == UnicodeCategory.OtherNumber)
                return false;
        }
        return true;
    }
}
