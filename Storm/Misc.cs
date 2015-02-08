using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
                System.Diagnostics.Process.Start(uri.AbsoluteUri);
            }
            else
            {
                throw new ArgumentException("urlString passed to Uri.TryCreate returns false");
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
                System.Diagnostics.Process.Start(uri.AbsoluteUri);
            }
            else
            {
                throw new ArgumentException("uri is not absolute");
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
        /// Logs a message to logfile.txt in user's Documents directory.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void LogMessage(string message)
        {
            string logFilePath = string.Format(@"C:\Users\{0}\Documents\logfile.txt", Environment.UserName);

            StringBuilder logMessage = new StringBuilder();

            logMessage.AppendLine(string.Format("{0} logged the following message at {1}:", Application.Current.ToString(), DateTime.Now));
            logMessage.AppendLine(message);
            logMessage.AppendLine(Environment.NewLine);

            using (FileStream fs = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.None, 4096, false))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(logMessage.ToString());
                }
            }
        }

        /// <summary>
        /// Asynchronously logs a message to logfile.txt in user's Documents directory.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static async Task LogMessageAsync(string message)
        {
            string logFilePath = string.Format(@"C:\Users\{0}\Documents\logfile.txt", Environment.UserName);

            StringBuilder logMessage = new StringBuilder();

            logMessage.AppendLine(string.Format("{0} logged the following message at {1}:", Application.Current.ToString(), DateTime.Now));
            logMessage.AppendLine(message);
            logMessage.AppendLine(Environment.NewLine);

            using (FileStream fs = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.None, 4096, true))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    await sw.WriteAsync(logMessage.ToString());
                }
            }
        }

        /// <summary>
        /// Logs a DispatcherUnhandledException's Exception property to a file.
        /// </summary>
        /// <param name="e">The exception object within the DispatcherUnhandledException.</param>
        public static void LogException(Exception e, string customMessage)
        {
            string logFilePath = string.Format(@"C:\Users\{0}\Documents\logfile.txt", Environment.UserName);

            StringBuilder logMessage = new StringBuilder();

            logMessage.AppendLine(string.Format("{0} occurred in {1} at {2}", e.GetType().ToString(), Application.Current.ToString(), DateTime.Now));

            if (String.IsNullOrWhiteSpace(customMessage) == false)
            {
                logMessage.AppendLine(customMessage);
            }

            logMessage.AppendLine(e.Message);
            logMessage.AppendLine(e.StackTrace);
            logMessage.AppendLine(Environment.NewLine);

            using (FileStream fs = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.None, 4096, false))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(logMessage.ToString());
                }
            }
        }

        /// <summary>
        /// Asynchronously logs an Exception to a file.
        /// </summary>
        /// <param name="e">The exception to log.</param>
        public async static Task LogExceptionAsync(Exception e, string customMessage)
        {
            string logFilePath = string.Format(@"C:\Users\{0}\Documents\logfile.txt", Environment.UserName);

            StringBuilder logMessage = new StringBuilder();

            logMessage.AppendLine(string.Format("{0} occurred in {1} at {2}", e.GetType().ToString(), Application.Current.ToString(), DateTime.Now));

            if (String.IsNullOrWhiteSpace(customMessage) == false)
            {
                logMessage.AppendLine(customMessage);
            }

            logMessage.AppendLine(e.Message);
            logMessage.AppendLine(e.StackTrace);
            logMessage.AppendLine(Environment.NewLine);

            using (FileStream fs = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.None, 4096, true))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    await sw.WriteAsync(logMessage.ToString());
                }
            }
        }

        /// <summary>
        /// Adds each item from an IEnumerable to a Collection.
        /// </summary>
        /// <typeparam name="T">The type of item in your collection.</typeparam>
        /// <param name="collection">The Collection to add the items to.</param>
        /// <param name="list">The IEnumerable of type T to add to the Collection.</param>
        public static void AddList<T>(this ICollection<T> collection, IEnumerable<T> list)
        {
            foreach (T obj in list)
            {
                collection.Add(obj);
            }
        }

        /// <summary>
        /// Adds all items that aren't already present in the Collection.
        /// We enforce that T implement IEquatable T because otherwise Collection T.Contains uses Object.Contains,
        /// which is only a reference equals.
        /// </summary>
        /// <typeparam name="T">The type of item in your collection.</typeparam>
        /// <param name="collection">The collection to add the items to.</param>
        /// <param name="list">The IEnumerable of T to add.</param>
        public static void AddMissingItems<T>(this ICollection<T> collection, IEnumerable<T> list) where T : IEquatable<T>
        {
            foreach (T each in list)
            {
                if (collection.Contains(each) == false)
                {
                    collection.Add(each);
                }
            }
        }

        /// <summary>
        /// Adds all items that aren't already present in the Collection.
        /// </summary>
        /// <typeparam name="T">The type of item in your collection.</typeparam>
        /// <param name="collection">The collection to add the items to.</param>
        /// <param name="list">The IEnumerable of T to add.</param>
        /// <param name="comparer">The IEnumerable of T to add.</param>
        public static void AddMissingItems<T>(this ICollection<T> collection, IEnumerable<T> list, IEqualityComparer<T> comparer)
        {
            foreach (T each in list)
            {
                if (collection.Contains<T>(each, comparer) == false)
                {
                    collection.Add(each);
                }
            }
        }

        /// <summary>
        /// Removes an IEnumerable's worth of T from an ICollection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection to remove from.</param>
        /// <param name="list">The items to remove from the collection.</param>
        public static void RemoveList<T>(this ICollection<T> collection, IEnumerable<T> list)
        {
            foreach (T obj in list)
            {
                collection.Remove(obj);
            }
        }

        /// <summary>
        /// Returns the string from between two parts of a string.
        /// </summary>
        /// <param name="whole">The string to search within.</param>
        /// <param name="beginning">Should end immediately before what we want back.</param>
        /// <param name="ending">Should begin immediately after what we want back.</param>
        /// <returns></returns>
        public static string FromBetween(this string whole, string beginning, string ending)
        {
            if (whole.Contains(beginning) == false)
            {
                //throw new ArgumentException(string.Format("beginning ({0}) does not appear within whole ({1})", beginning, whole));

                return string.Format("beginning ({0}) does not appear within whole ({1})", beginning, whole);
            }

            if (whole.Contains(ending) == false)
            {
                //throw new ArgumentException(string.Format("ending ({0}) does not appear within whole ({1})", ending, whole));

                return string.Format("ending ({0}) does not appear within whole ({1})", ending, whole);
            }

            if (whole.IndexOf(beginning) < 0)
            {
                //throw new ArgumentOutOfRangeException(string.Format("beginning ({0}) does not seem to appear in whole ({1})", beginning, whole));

                return string.Format("beginning ({0}) does not seem to appear in whole ({1})", beginning, whole);
            }

            if (whole.IndexOf(ending) < 0)
            {
                //throw new ArgumentOutOfRangeException(string.Format("ending ({0}) does not seem to appear within whole ({1})", ending, whole));

                return string.Format("ending ({0}) does not seem to appear within whole ({1})", ending, whole);
            }

            int beginningOfString = whole.IndexOf(beginning) + beginning.Length;
            int endingOfString = whole.IndexOf(ending);

            int length = endingOfString - beginningOfString;

            return whole.Substring(beginningOfString, length);
        }

        /// <summary>
        /// Returns a WebResponse that deals with WebException inside.
        /// </summary>
        /// <param name="req">The HttpWebRequest to perform.</param>
        /// <param name="maxRounds">The maximum number of times to attempt the operation with a hardcoded (blocking) 3.5s delay between attempts.</param>
        /// <returns></returns>
        public static WebResponse GetResponseExt(this WebRequest req, int maxRounds)
        {
            WebResponse webResp = null;
            bool tryAgain = false;

            try
            {
                webResp = req.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    webResp = e.Response;
                }
                else
                {
                    tryAgain = true;
                }

                string message = string.Format("Request uri: {0}, Tries left: {1}, Method: {2}, Timeout: {3}", req.RequestUri, maxRounds - 1, req.Method, req.Timeout);

                Misc.LogException(e, message);

                if (maxRounds == 1)
                {
                    return webResp;
                }
            }

            if (tryAgain)
            {
                System.Threading.Thread.Sleep(3500);

                webResp = GetResponseExt(req, maxRounds - 1);
            }

            return webResp;
        }

        /// <summary>
        /// Asynchronously returns a WebResponse that deals with WebException inside.
        /// </summary>
        /// <param name="req">The HttpWebRequest to perform.</param>
        /// <param name="maxRounds">The maximum number of times to attempt the operation with a hardcoded 3.5s delay between attempts.</param>
        /// <returns></returns>
        public static async Task<WebResponse> GetResponseAsyncExt(this WebRequest req, int maxRounds)
        {
            WebResponse webResp = null;
            bool tryAgain = false;

            try
            {
                webResp = await req.GetResponseAsync().ConfigureAwait(false);
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    webResp = e.Response;
                }
                else
                {
                    tryAgain = true;
                }

                string message = string.Format("Request uri: {0}, Tries left: {1}, Method: {2}, Timeout: {3}", req.RequestUri, maxRounds - 1, req.Method, req.Timeout);

                Misc.LogException(e, message);

                if (maxRounds == 1)
                {
                    return webResp;
                }
            }

            if (tryAgain)
            {
                await Task.Delay(3500);

                webResp = await GetResponseAsyncExt(req, maxRounds - 1).ConfigureAwait(false);
            }

            return webResp;
        }

        /// <summary>
        /// Downloads a request as a string.
        /// </summary>
        /// <param name="req">Custom HttpWebRequest object.</param>
        /// <returns></returns>
        public static async Task<string> DownloadWebsiteAsString(HttpWebRequest req, int maxRounds)
        {
            string response = string.Empty;

            using (HttpWebResponse resp = (HttpWebResponse)(await req.GetResponseAsyncExt(maxRounds)))
            {
                if (resp != null)
                {
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                        {
                            // documentation says this doesn't throw an IOException, but one comes from deeper in the framework
                            try
                            {
                                response = await sr.ReadToEndAsync().ConfigureAwait(false);
                            }
                            catch (IOException)
                            {
                                response = string.Empty;
                            }
                        }
                    }
                }
            }

            return response;
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

        /// <summary>
        /// Sets a window to the centre of the primary screen.
        /// </summary>
        /// <param name="window">The window you want to centre.</param>
        public static void SetWindowToMiddleOfScreen(Window window)
        {
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowHeight = window.Height;
            window.Top = (screenHeight / 2) - (windowHeight / 2);

            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double windowWidth = window.Width;
            window.Left = (screenWidth / 2) - (windowWidth / 2);
        }

        /// <summary>
        /// Removes CR+LF, LF, CR and Environment.NewLine from a string in that order.
        /// </summary>
        /// <param name="s">String to cleanse of filth.</param>
        /// <returns></returns>
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
    }
	
	/// <summary>
    /// Basic EventArgs class with only a string parameter.
    /// </summary>
	public class MessageEventArgs : EventArgs
	{
		public string Message { get; private set; }

        public MessageEventArgs(string message)
        {
            this.Message = message;
        }
	}
}