using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Storm.Extensions;

namespace Storm
{
    public static class Utils
    {
        private static int loggingRounds = 5;
        private static string logFilePath = string.Format(CultureInfo.CurrentCulture, @"C:\Users\{0}\Documents\logfile.txt", Environment.UserName);
        

        public static void SetWindowToMiddleOfScreen(Window window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));

            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowHeight = window.Height;
            window.Top = (screenHeight / 2) - (windowHeight / 2);

            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double windowWidth = window.Width;
            window.Left = (screenWidth / 2) - (windowWidth / 2);
        }

        public static void SafeDispatcher(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            Dispatcher disp = Application.Current.Dispatcher;

            if (disp.CheckAccess())
            {
                action();
            }
            else
            {
                disp.Invoke(action, DispatcherPriority.Normal);
            }
        }

        public static void SafeDispatcher(Action action, DispatcherPriority priority)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            Dispatcher disp = Application.Current.Dispatcher;

            if (disp.CheckAccess())
            {
                action();
            }
            else
            {
                disp.Invoke(action, priority);
            }
        }

        public static async Task SafeDispatcherAsync(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            Dispatcher disp = Application.Current.Dispatcher;

            if (disp.CheckAccess())
            {
                await Task.Run(() => action);
            }
            else
            {
                await disp.InvokeAsync(action, DispatcherPriority.Normal);
            }
        }

        public static async Task SafeDispatcherAsync(Action action, DispatcherPriority priority)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            Dispatcher disp = Application.Current.Dispatcher;

            if (disp.CheckAccess())
            {
                await Task.Run(() => action);
            }
            else
            {
                await disp.InvokeAsync(action, priority);
            }
        }

        
        public static void OpenUriInBrowser(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            if (uri.IsAbsoluteUri)
            {
                Process.Start(uri.AbsoluteUri);
            }
            else
            {
                string errorMessage = string.Format(CultureInfo.CurrentCulture, "Uri ({0}) was not absolute", uri.OriginalString);

                LogMessage(errorMessage);
            }
        }

        public static void OpenUriInBrowser(string uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            Uri tmp = null;

            if (Uri.TryCreate(uri, UriKind.Absolute, out tmp))
            {
                OpenUriInBrowser(tmp);
            }
            else
            {
                string errorMessage = string.Format(CultureInfo.CurrentCulture, "Uri.TryCreate returned false on {0}", uri);

                LogMessage(errorMessage);
            }
        }


        public static void LogMessage(string message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "{0} logged the following message at {1}", Process.GetCurrentProcess().MainModule.ModuleName, DateTime.Now));
            sb.AppendLine(message);

            WriteTextToFile(sb.ToString(), loggingRounds);
        }

        public static async Task LogMessageAsync(string message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "{0} logged the following message at {1}", Process.GetCurrentProcess().MainModule.ModuleName, DateTime.Now));
            sb.AppendLine(message);

            await WriteTextToFileAsync(sb.ToString(), loggingRounds).ConfigureAwait(false);
        }


        public static void LogException(Exception ex)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "{0} occurred in {1} at {2}", ex.GetType().ToString(), Process.GetCurrentProcess().MainModule.ModuleName, DateTime.Now));
            sb.AppendLine(ex.Message);
            sb.AppendLine(ex.StackTrace);

            WriteTextToFile(sb.ToString(), loggingRounds);
        }

        public static void LogException(Exception ex, string message)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));
            if (message == null) throw new ArgumentNullException(nameof(message));

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "{0} occurred in {1} at {2}", ex.GetType().ToString(), Process.GetCurrentProcess().MainModule.ModuleName, DateTime.Now));
            sb.AppendLine(message);
            sb.AppendLine(ex.Message);
            sb.AppendLine(ex.StackTrace);

            WriteTextToFile(sb.ToString(), loggingRounds);
        }

        public static async Task LogExceptionAsync(Exception ex)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "{0} occurred in {1} at {2}", ex.GetType().ToString(), Process.GetCurrentProcess().MainModule.ModuleName, DateTime.Now));
            sb.AppendLine(ex.Message);
            sb.AppendLine(ex.StackTrace);

            await WriteTextToFileAsync(sb.ToString(), loggingRounds).ConfigureAwait(false);
        }

        public static async Task LogExceptionAsync(Exception ex, string message)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));
            if (message == null) throw new ArgumentNullException(nameof(message));

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "{0} occurred in {1} at {2}", ex.GetType().ToString(), Process.GetCurrentProcess().MainModule.ModuleName, DateTime.Now));
            sb.AppendLine(message);
            sb.AppendLine(ex.Message);
            sb.AppendLine(ex.StackTrace);

            await WriteTextToFileAsync(sb.ToString(), loggingRounds).ConfigureAwait(false);
        }


        private static void WriteTextToFile(string text, int rounds = 1)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (rounds < 1) throw new ArgumentException("WriteTextToFile: rounds cannot be < 1");

            bool tryAgain = false;
            
            FileStream fs = null;
            try
            {
                fs = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.None, 1024, false);

                using (StreamWriter sw = new StreamWriter(fs))
                {
                    fs = null;

                    sw.WriteLine(text);
                }
            }
            catch (IOException)
            {
                tryAgain = (rounds > 1);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Dispose();
                }
            }

            if (tryAgain)
            {
                int variation = DateTime.UtcNow.Millisecond;

                /*
                 * we want the delay to increase as the number of attempts left decreases
                 * as rounds increases, (1 / rounds) decreases
                 * => as (1 / rounds) decreases, (25 / (1 / rounds)) increases 
                 * 
                 * we convert rounds to decimal because otherwise it would do integer division
                 * e.g. 1 / 3 = 0
                 */
                decimal fixedWait = 25 / (1 / Convert.ToDecimal(rounds));

                int toWait = Convert.ToInt32(fixedWait) + variation;

                Thread.Sleep(toWait);

                WriteTextToFile(text, rounds - 1);
            }
        }

        private static async Task WriteTextToFileAsync(string text, int rounds = 1)
        {
            if (rounds < 1) throw new ArgumentException("WriteTextToFileAsync: rounds cannot be < 1");

            bool tryAgain = false;
            
            FileStream fsAsync = null;
            try
            {
                fsAsync = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.None, 1024, true);

                using (StreamWriter sw = new StreamWriter(fsAsync))
                {
                    fsAsync = null;

                    await sw.WriteLineAsync(text).ConfigureAwait(false);
                }
            }
            catch (IOException)
            {
                tryAgain = (rounds > 1);
            }
            finally
            {
                if (fsAsync != null)
                {
                    fsAsync.Dispose();
                }
            }

            if (tryAgain)
            {
                int variation = DateTime.UtcNow.Millisecond;

                /*
                 * we want the delay to increase as the number of attempts left decreases
                 * as rounds increases, (1 / rounds) decreases
                 * => as (1 / rounds) decreases, (25 / (1 / rounds)) increases 
                 * 
                 * we convert rounds to decimal because otherwise it would do integer division
                 * e.g. 1 / 3 = 0
                 */
                decimal fixedWait = 25 / (1 / Convert.ToDecimal(rounds));

                int toWait = Convert.ToInt32(fixedWait) + variation;

                await Task.Delay(toWait).ConfigureAwait(false);

                await WriteTextToFileAsync(text, rounds - 1).ConfigureAwait(false);
            }
        }


        public static string DownloadWebsiteAsString(HttpWebRequest request)
        {
            StringBuilder sbLog = new StringBuilder();

            string response = string.Empty;

            using (HttpWebResponse resp = (HttpWebResponse)request.GetResponseExt())
            {
                if (resp == null)
                {
                    if (request != null)
                    {
                        request.Abort();
                    }
                }
                else
                {
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                        {
                            try
                            {
                                response = sr.ReadToEnd();
                            }
                            catch (IOException e)
                            {
                                sbLog.AppendLine(string.Format(CultureInfo.CurrentCulture, "Reading the response failed with IOException: {0}", request.RequestUri.AbsoluteUri));
                                sbLog.AppendLine(e.Message);
                                sbLog.AppendLine(e.StackTrace);

                                response = string.Empty;
                            }
                        }
                    }
                    else if (resp.StatusCode != HttpStatusCode.BadGateway)
                    {
                        sbLog.AppendLine(string.Format(CultureInfo.CurrentCulture, "Getting website {0} failed: {1}", request.RequestUri.AbsoluteUri, resp.StatusCode.ToString()));
                    }
                }
            }

            if (sbLog.Length > 0)
            {
                Utils.LogMessage(sbLog.ToString());
            }

            return response;
        }

        public static async Task<string> DownloadWebsiteAsStringAsync(HttpWebRequest request)
        {
            StringBuilder sbLog = new StringBuilder();

            string response = string.Empty;

            using (HttpWebResponse resp = (HttpWebResponse)(await request.GetResponseAsyncExt().ConfigureAwait(false)))
            {
                if (resp == null)
                {
                    request?.Abort();
                }
                else
                {
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                        {
                            try
                            {
                                response = await sr.ReadToEndAsync().ConfigureAwait(false);
                            }
                            catch (Exception e)
                            {
                                sbLog.AppendLine(string.Format("Reading the response failed with exception of type {0}", e.GetType().ToString()));
                                sbLog.AppendLine(e.Message);
                                sbLog.AppendLine(e.StackTrace);

                                response = string.Empty;
                            }
                        }
                    }
                    else if (resp.StatusCode != HttpStatusCode.BadGateway)
                    {
                        sbLog.AppendLine(string.Format("Getting website {0} failed: {1}", request.RequestUri.AbsoluteUri, resp.StatusCode.ToString()));
                    }
                }
            }

            if (sbLog.Length > 0)
            {
                await Utils.LogMessageAsync(sbLog.ToString()).ConfigureAwait(false);
            }

            return response;
        }
    }
}