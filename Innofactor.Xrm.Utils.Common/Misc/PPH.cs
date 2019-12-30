namespace Innofactor.Xrm.Utils.Common.Misc
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Populate PlaceHolders extensions for SDK Entity class
    /// </summary>
    public static class PPH
    {
        // List of possible special format tags to use when formatting data
        private static List<string> extraFormatTags = new List<string>() { "MaxLen", "Pad", "Left", "Right", "SubStr", "Replace", "Math" };
        internal static string FormatByTag(string text, string formatTag)
        {
            if (formatTag.StartsWith("MaxLen", StringComparison.Ordinal))
            {
                text = FormatLeft(text, formatTag.Replace("MaxLen", "Left").Replace("Left:", "Left|"));   // A few replace for backward compatibility
            }
            else if (formatTag.StartsWith("Left", StringComparison.Ordinal))
            {
                text = FormatLeft(text, formatTag);
            }
            else if (formatTag.StartsWith("Right", StringComparison.Ordinal))
            {
                text = FormatRight(text, formatTag);
            }
            else if (formatTag.StartsWith("SubStr", StringComparison.Ordinal))
            {
                text = FormatSubStr(text, formatTag);
            }
            else if (formatTag.StartsWith("Math", StringComparison.Ordinal))
            {
                text = FormatMath(text, formatTag);
            }
            else if (formatTag.StartsWith("Pad", StringComparison.Ordinal))
            {
                text = FormatPad(text, formatTag);
            }
            else if (formatTag.StartsWith("Replace", StringComparison.Ordinal))
            {
                text = FormatReplace(text, formatTag);
            }

            return text;
        }
        internal static string ExtractExtraFormatTags(string format, List<string> extraFormats)
        {
            while (ContainsAnyTag(format))
            {
                var pos = int.MaxValue;
                var nextFormat = string.Empty;
                foreach (var tag in extraFormatTags)
                {
                    var extraFormat = GetFirstEnclosedPart(format, "<", tag, ">", "");
                    if (!string.IsNullOrEmpty(extraFormat) && format.IndexOf(extraFormat, StringComparison.Ordinal) < pos)
                    {
                        nextFormat = extraFormat;
                        pos = format.IndexOf(extraFormat, StringComparison.Ordinal);
                    }
                }
                if (!string.IsNullOrEmpty(nextFormat))
                {
                    extraFormats.Add(nextFormat);
                    format = format.Replace("<" + nextFormat + ">", "");
                }
            }

            return format;
        }

       
        /// <summary>
        /// Compares positions of item1 and item2 in source
        /// </summary>
        /// <param name="source">Stringto be investigated</param>
        /// <param name="item1">First string to find</param>
        /// <param name="item2">Second string to find</param>
        /// <returns>
        /// 0  - Neither items exist in source
        /// &lt;0 - Only item1 exists or item1 occurs before item2
        /// &gt;0 - Only item2 exists or item1 occurs after item2
        /// </returns>
        private static int ComparePositions(string source, string item1, string item2) =>
            (source + item1).IndexOf(item1, StringComparison.Ordinal) - (source + item2).IndexOf(item2, StringComparison.Ordinal);

        private static bool ContainsAnyTag(string format)
        {
            if (!string.IsNullOrEmpty(format))
            {
                foreach (var tag in extraFormatTags)
                {
                    if (format.Contains("<" + tag + "|") && format.Contains(">"))
                    {
                        return true;
                    }
                    if (format.Contains("<" + tag + "=") && format.Contains(">"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static string FormatLeft(string text, string formatTag)
        {
            var lenstr = GetSeparatedPart(formatTag, "|", 2);
            if (!int.TryParse(lenstr, out var len))
            {
                throw new InvalidPluginExecutionException("PPH left length must be a positive integer (" + lenstr + ")");
            }
            if (text.Length > len)
            {
                text = text.Substring(0, len);
            }
            return text;
        }

        private static string FormatMath(string text, string formatTag)
        {
            if (!decimal.TryParse(text.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator).Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator), out var textvalue))
            {
                throw new InvalidPluginExecutionException("PPH math text must be a valid decimal number (" + text + ")");
            }
            var oper = GetSeparatedPart(formatTag, "|", 2).ToUpperInvariant();
            decimal value = 0;
            if (oper != "round" && oper != "abs")
            {
                var valuestr = GetSeparatedPart(formatTag, "|", 3);
                if (!decimal.TryParse(valuestr, out value))
                {
                    throw new InvalidPluginExecutionException("PPH math value must be a valid decimal number (" + valuestr + ")");
                }
            }
            switch (oper)
            {
                case "+":
                    textvalue = textvalue + value;
                    break;

                case "-":
                    textvalue = textvalue - value;
                    break;

                case "*":
                    textvalue = textvalue * value;
                    break;

                case "/":
                    textvalue = textvalue / value;
                    break;

                case "DIV":
                    int rem;
                    textvalue = Math.DivRem((int)textvalue, (int)value, out rem);
                    break;

                case "MOD":
                    int remainder;
                    Math.DivRem((int)textvalue, (int)value, out remainder);
                    textvalue = remainder;
                    break;

                case "ROUND":
                    textvalue = Math.Round(textvalue);
                    break;

                case "ABS":
                    textvalue = Math.Abs(textvalue);
                    break;

                default:
                    throw new InvalidPluginExecutionException("PPH math operator not valid (" + oper + ")");
            }
            return textvalue.ToString(CultureInfo.InvariantCulture);
        }

        private static string FormatPad(string text, string formatTag)
        {
            var dir = GetSeparatedPart(formatTag, "|", 2);
            if (dir != "R" && dir != "L")
            {
                throw new InvalidPluginExecutionException("PPH pad direction must be R or L");
            }
            var lenstr = GetSeparatedPart(formatTag, "|", 3);
            if (!int.TryParse(lenstr, out var len))
            {
                throw new InvalidPluginExecutionException("PPH pad length must be a positive integer (" + lenstr + ")");
            }
            var pad = GetSeparatedPart(formatTag, "|", 4);
            if (string.IsNullOrEmpty(pad))
            {
                pad = " ";
            }
            while (text.Length < len)
            {
                switch (dir)
                {
                    case "R": text = $"{text}{pad}"; break;
                    case "L": text = $"{pad}{text}"; break;
                }
            }

            return text;
        }

        private static string FormatReplace(string text, string formatTag)
        {
            var oldText = GetSeparatedPart(formatTag, "|", 2);
            if (string.IsNullOrEmpty(oldText))
            {
                throw new InvalidPluginExecutionException("PPH replace old must be non-empty");
            }
            var newText = GetSeparatedPart(formatTag, "|", 3);
            text = text.Replace(oldText, newText);
            return text;
        }

        private static string FormatRight(string text, string formatTag)
        {
            var lenstr = GetSeparatedPart(formatTag, "|", 2);
            if (!int.TryParse(lenstr, out var len))
            {
                throw new InvalidPluginExecutionException("PPH right length must be a positive integer (" + lenstr + ")");
            }
            if (text.Length > len)
            {
                text = text.Substring(text.Length - len);
            }
            return text;
        }

        private static string FormatSubStr(string text, string formatTag)
        {
            var startstr = GetSeparatedPart(formatTag, "|", 2);
            if (!int.TryParse(startstr, out var start))
            {
                throw new InvalidPluginExecutionException("PPH substr start must be a positive integer (" + startstr + ")");
            }
            var lenstr = GetSeparatedPart(formatTag, "|", 3);
            if (!string.IsNullOrEmpty(lenstr))
            {
                if (!int.TryParse(lenstr, out var len))
                {
                    throw new InvalidPluginExecutionException("PPH substr length must be a positive integer (" + lenstr + ")");
                }
                text = text.Substring(start, len);
            }
            else
            {
                text = text.Substring(start);
            }
            return text;
        }

        private static string GetFirstEnclosedPart(string source, string starttag, string keyword, string endtag, string entitynamespace)
        {
            var startidentifier = starttag + entitynamespace + keyword;
            if (!source.Contains(startidentifier) || !source.Contains(endtag))
            {   // Felaktiga start/end eller keyword
                return "";
            }
            if (string.IsNullOrEmpty(entitynamespace)
                && ComparePositions(source, "{", ":") < 0   // Startkrull före kolon
                && ComparePositions(source, "}", ":") > 0   // Slutkrull före kolon
                && ComparePositions(source, ":", "<") < 0   // Kolon före starttag
                && ComparePositions(source, ":", "|") < 0)  // Kolon före format-pipe
            {   // Det finns ett kolon som avser namespace, men inget namespace var angivet
                return "";
            }
            var result = source.Substring(source.IndexOf(startidentifier, StringComparison.Ordinal) + 1);
            var tagcount = 1;
            var pos = 0;
            while (pos < result.Length && tagcount > 0)
            {
                if (result.Substring(pos).StartsWith(endtag, StringComparison.Ordinal))
                {
                    tagcount--;
                }
                else if (result.Substring(pos).StartsWith(starttag, StringComparison.Ordinal))
                {
                    tagcount++;
                }

                pos++;
            }
            if (tagcount > 0)
            {
                throw new InvalidOperationException("GetFirstEnclosedPart: Missing end tag: " + endtag);
            }

            result = result.Substring(0, pos - 1);
            return result;
        }
        private static string GetSeparatedPart(string source, string separator, int partno)
        {
            var tagcount = 0;
            var pos = 0;
            var separatorcount = 0;
            var startpos = -1;
            while (separatorcount < partno && pos < source.Length)
            {
                if (startpos == -1 && separatorcount == partno - 1 && tagcount == 0)
                {
                    startpos = pos;
                }

                var character = source[pos];
                if (character == '>' || character == '}')
                {
                    tagcount--;
                }
                else if (character == '<' || character == '{')
                {
                    tagcount++;
                }

                if (tagcount == 0 && source.Substring(pos).StartsWith(separator, StringComparison.Ordinal))
                {
                    separatorcount++;
                }

                pos++;
            }
            var length = pos == source.Length ? pos - startpos : pos - startpos - 1;
            var result = "";
            if (startpos >= 0 && startpos + length <= source.Length)
            {
                result = source.Substring(startpos, length);
            }

            if (result.EndsWith(separator, StringComparison.Ordinal))
            {   // Special case when the complete placeholder ends with the separator
                result = result.Substring(0, result.Length - separator.Length);
            }
            return result;
        }
    }
}