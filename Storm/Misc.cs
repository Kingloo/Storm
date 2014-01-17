using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Storm
{
    /// <summary>
    /// Miscellaneous functions and extensions.
    /// </summary>
    public static class Misc
    {
        /// <summary>
        /// Trys to navigate to a string Url address in the default browser, then in Internet Explorer.
        /// </summary>
        /// <param name="urlString">The address as a string.</param>
        public static void OpenUrlInBrowser(string urlString)
        {
            Uri uri = null;

            if (Uri.TryCreate(urlString, UriKind.Absolute, out uri))
            {
                try
                {
                    System.Diagnostics.Process.Start(urlString);
                }
                catch (System.IO.FileNotFoundException)
                {
                    System.Diagnostics.Process.Start("iexplore.exe", urlString);
                }
            }
            else
            {
                MessageBox.Show("String does not appear to be a valid URL.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Trys to navigate to a Uri address in the default browser, then in Internet Explorer.
        /// </summary>
        /// <param name="uri">The Uri object.</param>
        public static void OpenUrlInBrowser(Uri uri)
        {
            if (uri.IsAbsoluteUri)
            {
                try
                {
                    System.Diagnostics.Process.Start(uri.AbsoluteUri);
                }
                catch (System.IO.FileNotFoundException)
                {
                    System.Diagnostics.Process.Start("iexplore.exe", uri.AbsoluteUri);
                }
            }
            else
            {
                MessageBox.Show("The Uri is a relative address. An absolute Uri is required.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// You want a string.Contains that you can tell about case and culture.
        /// </summary>
        /// <param name="target">The string to search within.</param>
        /// <param name="toFind">The string you want to find.</param>
        /// <param name="comp">A StringComparison used to determine Culture and Case options.</param>
        /// <returns>A boolean, TRUE means target does contain find and vice versa.</returns>
        public static bool ContainsExt(this string target, string toFind, StringComparison comp)
        {
            if (target.IndexOf(toFind, comp) >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// You have a string[] where each string ends in a number. Typical ordering will give you 0,1,10,...,2,20 etc. But you want 0,1,2,...,10,11,...20,21 etc.
        /// </summary>
        /// <param name="list">A string[] ordered incorrectly.</param>
        /// <param name="d">Positive int means 0,1,2,3..., negative int means 5,4,3,2,1,0, and 0 fails.</param>
        /// <returns>A string[] ordered properly.</returns>
        public static string[] OrderStringArray(this string[] list, int d)
        {
            int numberOfStrings = list.Count<string>();
            Dictionary<int, string> dict = new Dictionary<int, string>();

            foreach (string str in list)
            {
                dict.Add(getNumber(str), str);
            }

            IEnumerable<KeyValuePair<int, string>> ienum = null;

            if (d > 0)
            {
                ienum = dict.OrderBy(a => a.Key);
            }
            else if (d < 0)
            {
                ienum = dict.OrderByDescending(a => a.Key);
            }
            else
            {
                return new string[0];
            }

            string[] sorted = new string[dict.Count];

            int i = 0;
            foreach (KeyValuePair<int, string> kvp in ienum)
            {
                sorted[i] = kvp.Value;
                i++;
                Console.WriteLine("Dict: " + kvp.Key.ToString() + ", " + kvp.Value);
            }

            return sorted;
        }

        private static int getNumber(string str)
        {
            int index = 0;

            for (int i = str.Length - 1; i > 0; i--)
            {
                if (Char.IsLetter(str[i]) == true)
                {
                    index = i + 1;
                    break;
                }
            }

            return Convert.ToInt32(str.Substring(index));
        }
    }
}
