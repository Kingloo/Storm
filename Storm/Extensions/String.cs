using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Storm.Extensions
{
    public enum Result { None, Success, BeginningNotFound, BeginningNotUnique, EndingNotFound, EndingBeforeBeginning };

    public class FromBetweenResult
    {
        private Result _result = Result.None;
        public Result Result
        {
            get
            {
                return _result;
            }
        }

        private string _resultString = string.Empty;
        public string ResultString
        {
            get
            {
                return _resultString;
            }
        }

        public FromBetweenResult(Result result, string resultString)
        {
            this._result = result;
            this._resultString = resultString;
        }
    }

    public static class StringExt
    {
        public static bool ContainsExt(this string target, string toFind, StringComparison comparison)
        {
            return (target.IndexOf(toFind, comparison) > -1);
        }

        public static string RemoveNewLines(this string s)
        {
            string toReturn = s;

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

        public static FromBetweenResult FromBetween(this string whole, string beginning, string ending)
        {
            if (whole.Contains(beginning) == false)
            {
                return new FromBetweenResult(Result.BeginningNotFound, string.Empty);
            }

            int firstAppearanceOfBeginning = whole.IndexOf(beginning);
            int lastAppearanceOfBeginning = whole.LastIndexOf(beginning);

            if (firstAppearanceOfBeginning != lastAppearanceOfBeginning)
            {
                return new FromBetweenResult(Result.BeginningNotUnique, string.Empty);
            }

            int indexOfBeginning = firstAppearanceOfBeginning;

            if (whole.Contains(ending) == false)
            {
                return new FromBetweenResult(Result.EndingNotFound, string.Empty);
            }

            int indexOfEnding = 0;

            try
            {
                indexOfEnding = whole.IndexOf(ending, indexOfBeginning);
            }
            catch (ArgumentOutOfRangeException)
            {
                return new FromBetweenResult(Result.EndingBeforeBeginning, string.Empty);
            }

            int indexOfResult = indexOfBeginning + beginning.Length;
            int lengthOfResult = indexOfEnding - indexOfResult;

            string result = whole.Substring(indexOfResult, lengthOfResult);

            return new FromBetweenResult(Result.Success, result);
        }

        public static string RemoveUnicodeCategories(this string orig, IEnumerable<UnicodeCategory> categories)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in orig)
            {
                if (IsCharInCatergories(c, categories) == false)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        private static bool IsCharInCatergories(char c, IEnumerable<UnicodeCategory> categories)
        {
            foreach (UnicodeCategory category in categories)
            {
                if (Char.GetUnicodeCategory(c) == category)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
