using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
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
                throw new ArgumentException("beginning does not appear within whole", "beginning");
            }

            int firstAppearanceOfBeginning = whole.IndexOf(beginning);
            int lastAppearanceOfBeginning = whole.LastIndexOf(beginning);

            if (firstAppearanceOfBeginning != lastAppearanceOfBeginning)
            {
                throw new ArgumentException("beginning appears more than once within whole, it must be globally unique", "beginning");
            }

            if (whole.Contains(ending) == false)
            {
                throw new ArgumentException("ending does not appear within whole", "ending");
            }

            int indexOfBeginning = whole.IndexOf(beginning) + beginning.Length;

            // we start searching after beginning ends
            // in case ending is also within beginning
            int indexOfEnding = whole.IndexOf(ending, indexOfBeginning);

            int length = indexOfEnding - indexOfBeginning;

            return whole.Substring(indexOfBeginning, length);
        }


        // ICollection<T>
        public static void AddList<T>(this ICollection<T> collection, IEnumerable<T> list)
        {
            foreach (T obj in list)
            {
                collection.Add(obj);
            }
        }

        public static int AddMissing<T>(this ICollection<T> collection, IEnumerable<T> list) where T : IEquatable<T>
        {
            int countAdded = 0;

            foreach (T obj in list)
            {
                if (collection.Contains(obj) == false)
                {
                    collection.Add(obj);

                    countAdded++;
                }
            }

            return countAdded;
        }

        public static int AddMissing<T>(this ICollection<T> collection, IEnumerable<T> list, IEqualityComparer<T> comparer)
        {
            int countAdded = 0;

            foreach (T obj in list)
            {
                if (collection.Contains<T>(obj, comparer) == false)
                {
                    collection.Add(obj);

                    countAdded++;
                }
            }

            return countAdded;
        }

        public static void RemoveList<T>(this ICollection<T> collection, IEnumerable<T> list)
        {
            foreach (T obj in list)
            {
                collection.Remove(obj);
            }
        }


        // HttpWebRequest
        public static WebResponse GetResponseExt(this HttpWebRequest req, bool useLogging, int rounds = 1)
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

                webResp = GetResponseExt(req, useLogging, rounds - 1);
            }

            return webResp;
        }
        
        public static async Task<WebResponse> GetResponseAsyncExt(this HttpWebRequest req, bool useLogging, int rounds = 1)
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
                await Task.Delay(3000);

                webResp = await GetResponseAsyncExt(req, useLogging, rounds - 1).ConfigureAwait(false);
            }

            return webResp;
        }


        // Task
        public static async Task TimeoutAfter(this Task task, TimeSpan timeout)
        {
            if (task == Task.WhenAny(task, Task.Delay(timeout)))
            {
                await task;
            }
            else
            {
                throw new TimeoutException(string.Format("Task timed out: {0}", task.Status.ToString()));
            }
        }
    }
}