using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace StormLib.Extensions
{
    public static class StringExtensions
    {
        public static bool ContainsExt(this string target, string toFind, StringComparison comparison)
        {
            return (target.IndexOf(toFind, comparison) > -1);
        }

        public static string RemoveNewLines(this string value)
        {
            var sco = StringComparison.Ordinal;

            string toReturn = value;

            if (toReturn.ContainsExt("\r\n", sco))
            {
                toReturn = toReturn.Replace("\r\n", " ");
            }

            if (toReturn.ContainsExt("\r", sco))
            {
                toReturn = toReturn.Replace("\r", " ");
            }

            if (toReturn.ContainsExt("\n", sco))
            {
                toReturn = toReturn.Replace("\n", " ");
            }

            if (toReturn.ContainsExt(Environment.NewLine, sco))
            {
                toReturn = toReturn.Replace(Environment.NewLine, " ");
            }

            return toReturn;
        }
        
        public static string RemoveUnicodeCategories(this string self, IEnumerable<UnicodeCategory> categories)
        {
            var sb = new StringBuilder();

            foreach (char c in self)
            {
                if (!categories.Any(category => category == Char.GetUnicodeCategory(c)))
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        public static IReadOnlyCollection<string> FindBetween(this string text, string beginning, string ending)
        {
            if (String.IsNullOrEmpty(beginning)) { throw new ArgumentException("beginning was NullOrEmpty", nameof(beginning)); }
            if (String.IsNullOrEmpty(ending)) { throw new ArgumentException("ending was NullOrEmpty", nameof(ending)); }

            List<string> results = new List<string>();

            string pattern = string.Format(
                CultureInfo.CurrentCulture,
                "{0}({1}){2}",
                Regex.Escape(beginning),
                ".+?",
                Regex.Escape(ending));

            foreach (Match? m in Regex.Matches(text, pattern))
            {
                if (m != null)
				{
					results.Add(m.Groups[1].Value);
				}
            }

            return results;
        }

        public static string EnsureStartsWithHttps(this string input)
        {
            const string https = "https://";
            const string http = "http://";

            if (input.StartsWith(https, StringComparison.OrdinalIgnoreCase))
            {
                return input;
            }

            if (input.StartsWith(http, StringComparison.OrdinalIgnoreCase))
            {
                return input.Insert(4, "s");
            }

            return string.Format(CultureInfo.CurrentCulture, "{0}{1}", https, input);
        }
    }
}
