using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Storm
{
    public static class Extensions
    {
        // string
        public static bool ContainsExt(this string target, string toFind, StringComparison comparison)
        {
            if (target.IndexOf(toFind, comparison) >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
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

        public static string FromBetween(this string whole, string beginning, string ending)
        {
            if (whole.Contains(beginning) == false)
            {
                return string.Format("beginning ({0}) does not appear within whole ({1})", beginning, whole);
            }

            if (whole.Contains(ending) == false)
            {
                return string.Format("ending ({0}) does not appear within whole ({1})", ending, whole);
            }

            if (whole.IndexOf(beginning) < 0)
            {
                return string.Format("beginning ({0}) does not seem to appear in whole ({1})", beginning, whole);
            }

            if (whole.IndexOf(ending) < 0)
            {
                return string.Format("ending ({0}) does not seem to appear within whole ({1})", ending, whole);
            }

            int beginningOfString = whole.IndexOf(beginning) + beginning.Length;
            int endingOfString = whole.IndexOf(ending);

            int length = endingOfString - beginningOfString;

            return whole.Substring(beginningOfString, length);
        }

        // ICollection<T>
        public static void AddList<T>(this ICollection<T> collection, IEnumerable<T> list)
        {
            foreach (T obj in list)
            {
                collection.Add(obj);
            }
        }

        public static void AddMissing<T>(this ICollection<T> collection, IEnumerable<T> list) where T : IEquatable<T>
        {
            foreach (T obj in list)
            {
                if (collection.Contains(obj) == false)
                {
                    collection.Add(obj);
                }
            }
        }

        public static void AddMissing<T>(this ICollection<T> collection, IEnumerable<T> list, IEqualityComparer<T> comparer)
        {
            foreach (T obj in list)
            {
                if (collection.Contains<T>(obj, comparer) == false)
                {
                    collection.Add(obj);
                }
            }
        }

        public static void RemoveList<T>(this ICollection<T> collection, IEnumerable<T> list)
        {
            foreach (T obj in list)
            {
                collection.Remove(obj);
            }
        }

        // HttpWebRequest
        public static WebResponse GetResponseExt(this HttpWebRequest req)
        {
            WebResponse webResp = null;

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
            }

            return webResp;
        }

        public static WebResponse GetResponseExt(this HttpWebRequest req, int rounds)
        {
            if (rounds < 1) throw new ArgumentException("rounds cannot be < 1");

            WebResponse webResp = null;
            bool tryAgain = false;

            try
            {
                webResp = req.GetResponse();
            }
            catch (WebException e)
            {
                tryAgain = (rounds > 1);

                if (e.Response != null)
                {
                    webResp = e.Response;
                }
            }

            if (tryAgain)
            {
                System.Threading.Thread.Sleep(3500);

                webResp = GetResponseExt(req, rounds - 1);
            }

            return webResp;
        }

        public static WebResponse GetResponseExt(this HttpWebRequest req, int rounds, bool useLogging)
        {
            if (rounds < 1) throw new ArgumentException("rounds cannot be < 1");

            WebResponse webResp = null;
            bool tryAgain = false;

            try
            {
                webResp = req.GetResponse();
            }
            catch (WebException e)
            {
                tryAgain = (rounds > 1);

                if (e.Response != null)
                {
                    webResp = e.Response;
                }

                if (useLogging)
                {
                    string message = string.Format("Request uri: {0}, Tries left: {1}, Method: {2}, Timeout: {3}", req.RequestUri, rounds - 1, req.Method, req.Timeout);

                    Utils.LogException(e, message);
                }
            }

            if (tryAgain)
            {
                System.Threading.Thread.Sleep(3500);

                webResp = GetResponseExt(req, rounds - 1, useLogging);
            }

            return webResp;
        }
        
        public static async Task<WebResponse> GetResponseAsyncExt(this HttpWebRequest req)
        {
            WebResponse webResp = null;

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
            }

            return webResp;
        }

        public static async Task<WebResponse> GetResponseAsyncExt(this HttpWebRequest req, int rounds)
        {
            if (rounds < 1) throw new ArgumentException("rounds cannot be < 1");

            WebResponse webResp = null;
            bool tryAgain = false;

            try
            {
                webResp = await req.GetResponseAsync().ConfigureAwait(false);
            }
            catch (WebException e)
            {
                tryAgain = (rounds > 1);
                
                if (e.Response != null)
                {
                    webResp = e.Response;
                }
            }

            if (tryAgain)
            {
                await Task.Delay(3500);

                webResp = await GetResponseAsyncExt(req, rounds - 1).ConfigureAwait(false);
            }

            return webResp;
        }

        public static async Task<WebResponse> GetResponseAsyncExt(this HttpWebRequest req, int rounds, bool useLogging)
        {
            if (rounds < 1) throw new ArgumentException("rounds cannot be < 1");

            WebResponse webResp = null;
            bool tryAgain = false;

            try
            {
                webResp = await req.GetResponseAsync().ConfigureAwait(false);
            }
            catch (WebException e)
            {
                tryAgain = (rounds > 1);

                if (e.Response != null)
                {
                    webResp = e.Response;
                }

                if (useLogging)
                {
                    string message = string.Format("Request uri: {0}, Tries left: {1}, Method: {2}, Timeout: {3}", req.RequestUri, rounds - 1, req.Method, req.Timeout);

                    Utils.LogException(e, message);
                }
            }

            if (tryAgain)
            {
                await Task.Delay(3500);

                webResp = await GetResponseAsyncExt(req, rounds - 1, useLogging).ConfigureAwait(false);
            }

            return webResp;
        }
    }
}