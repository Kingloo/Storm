using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Storm.Extensions
{
    public static class StringExt
    {
        public static bool ContainsExt(this string target, string toFind, StringComparison comparison)
        {
            if (target == null) { throw new ArgumentNullException(nameof(target)); }

            return (target.IndexOf(toFind, comparison) > -1);
        }


        public static string RemoveNewLines(this string value)
        {
            if (value == null) { throw new ArgumentNullException(nameof(value)); }

            string toReturn = value;

            if (toReturn.Contains("\r\n"))
            {
                toReturn = toReturn.Replace("\r\n", " ");
            }

            if (toReturn.Contains("\r"))
            {
                toReturn = toReturn.Replace("\r", " ");
            }

            if (toReturn.Contains("\n"))
            {
                toReturn = toReturn.Replace("\n", " ");
            }

            if (toReturn.Contains(Environment.NewLine))
            {
                toReturn = toReturn.Replace(Environment.NewLine, " ");
            }

            return toReturn;
        }

        
        public static string RemoveUnicodeCategories(this string self, IEnumerable<UnicodeCategory> categories)
        {
            if (self == null) { throw new ArgumentNullException(nameof(self)); }
            if (categories == null) { throw new ArgumentNullException(nameof(categories)); }

            StringBuilder sb = new StringBuilder();

            foreach (char c in self)
            {
                if (!IsCharInCatergories(c, categories))
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        private static bool IsCharInCatergories(char c, IEnumerable<UnicodeCategory> categories)
        {
            if (categories == null) { throw new ArgumentNullException(nameof(categories)); }

            foreach (UnicodeCategory category in categories)
            {
                if (Char.GetUnicodeCategory(c) == category)
                {
                    return true;
                }
            }

            return false;
        }


        public static IReadOnlyList<string> FindBetween(this string text, string beginning, string ending)
        {
            if (text == null) { throw new ArgumentNullException(nameof(text)); }

            if (String.IsNullOrEmpty(beginning)) { throw new ArgumentException("beginning was NullOrEmpty", nameof(beginning)); }
            if (String.IsNullOrEmpty(ending)) { throw new ArgumentException("ending was NullOrEmpty", nameof(ending)); }

            List<string> results = new List<string>();

            string pattern = string.Format(
                CultureInfo.CurrentCulture,
                "{0}({1}){2}",
                Regex.Escape(beginning),
                ".+?",
                Regex.Escape(ending));

            foreach (Match m in Regex.Matches(text, pattern))
            {
                results.Add(m.Groups[1].Value);
            }

            return results;
        }
    }
}
