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
        private static int rounds = 5;
        private static string logFilePath = GetLogFilePath();

        private static string GetLogFilePath()
        {
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filename = "logfile.txt";

            return Path.Combine(dir, filename);
        }
        

        public static void SetWindowToMiddleOfScreen(Window window)
        {
            if (window == null) { throw new ArgumentNullException(nameof(window)); }

            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowHeight = window.Height;
            window.Top = (screenHeight / 2) - (windowHeight / 2);

            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double windowWidth = window.Width;
            window.Left = (screenWidth / 2) - (windowWidth / 2);
        }


        public static void SafeDispatcher(Action action)
        {
            if (action == null) { throw new ArgumentNullException(nameof(action)); }

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
            if (action == null) { throw new ArgumentNullException(nameof(action)); }

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
            if (action == null) { throw new ArgumentNullException(nameof(action)); }

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
            if (action == null) { throw new ArgumentNullException(nameof(action)); }

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
            if (uri == null) { throw new ArgumentNullException(nameof(uri)); }

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
            if (uri == null) { throw new ArgumentNullException(nameof(uri)); }

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
            if (message == null) { throw new ArgumentNullException(nameof(message)); }

            DateTime time = DateTime.Now;
            string name = Process.GetCurrentProcess().MainModule.ModuleName;

            string log = string.Format(CultureInfo.CurrentCulture, "{0} - {1} - {2}", time, name, message);

            WriteTextToFile(log, rounds);
        }

        public static async Task LogMessageAsync(string message)
        {
            if (message == null) { throw new ArgumentNullException(nameof(message)); }

            DateTime time = DateTime.Now;
            string name = Process.GetCurrentProcess().MainModule.ModuleName;

            string log = string.Format(CultureInfo.CurrentCulture, "{0} - {1} - {2}", time, name, message);
            
            await WriteTextToFileAsync(log, rounds).ConfigureAwait(false);
        }


        public static void LogException(Exception ex)
        {
            if (ex == null) { throw new ArgumentNullException(nameof(ex)); }

            StringBuilder sb = new StringBuilder();

            DateTime time = DateTime.Now;
            string name = Process.GetCurrentProcess().MainModule.ModuleName;
            string type = ex.GetType().ToString();
            
            string log = string.Format(CultureInfo.CurrentCulture, "{0} - {1} - {2}", time, name, type);

            sb.AppendLine(log);
            sb.AppendLine(ex.Message);
            sb.AppendLine(ex.StackTrace);
            
            WriteTextToFile(sb.ToString(), rounds);
        }

        public static void LogException(Exception ex, string message)
        {
            if (ex == null) { throw new ArgumentNullException(nameof(ex)); }
            if (message == null) { throw new ArgumentNullException(nameof(message)); }

            StringBuilder sb = new StringBuilder();

            DateTime time = DateTime.Now;
            string name = Process.GetCurrentProcess().MainModule.ModuleName;
            string type = ex.GetType().ToString();

            string log = string.Format(CultureInfo.CurrentCulture, "{0} - {1} - {2} - {3}", time, name, type, message);
            
            sb.AppendLine(log);
            sb.AppendLine(ex.Message);
            sb.AppendLine(ex.StackTrace);

            WriteTextToFile(sb.ToString(), rounds);
        }

        public static async Task LogExceptionAsync(Exception ex)
        {
            if (ex == null) { throw new ArgumentNullException(nameof(ex)); }

            StringBuilder sb = new StringBuilder();

            DateTime time = DateTime.Now;
            string name = Process.GetCurrentProcess().MainModule.ModuleName;
            string type = ex.GetType().ToString();

            string log = string.Format(CultureInfo.CurrentCulture, "{0} - {1} - {2}", time, name, type);

            sb.AppendLine(log);
            sb.AppendLine(ex.Message);
            sb.AppendLine(ex.StackTrace);

            await WriteTextToFileAsync(sb.ToString(), rounds).ConfigureAwait(false);
        }

        public static async Task LogExceptionAsync(Exception ex, string message)
        {
            if (ex == null) { throw new ArgumentNullException(nameof(ex)); }
            if (message == null) { throw new ArgumentNullException(nameof(message)); }

            StringBuilder sb = new StringBuilder();

            DateTime time = DateTime.Now;
            string name = Process.GetCurrentProcess().MainModule.ModuleName;
            string type = ex.GetType().ToString();

            string log = string.Format(CultureInfo.CurrentCulture, "{0} - {1} - {2} - {3}", time, name, type, message);

            sb.AppendLine(log);
            sb.AppendLine(ex.Message);
            sb.AppendLine(ex.StackTrace);

            await WriteTextToFileAsync(sb.ToString(), rounds).ConfigureAwait(false);
        }


        private static void WriteTextToFile(string text, int rounds = 1)
        {
            if (text == null) { throw new ArgumentNullException(nameof(text)); }
            if (rounds < 1) { throw new ArgumentException("WriteTextToFile: rounds cannot be < 1"); }

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
                fs?.Dispose();
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
            if (text == null) { throw new ArgumentNullException(nameof(text)); }
            if (rounds < 1) { throw new ArgumentException("WriteTextToFileAsync: rounds cannot be < 1"); }

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
                fsAsync?.Dispose();
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
            if (request == null) { throw new ArgumentNullException(nameof(request)); }

            string response = string.Empty;

            using (HttpWebResponse resp = (HttpWebResponse)request.GetResponseExt())
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
                                response = sr.ReadToEnd();
                            }
                            catch (Exception e)
                            {
                                string message = string.Format(CultureInfo.CurrentCulture, "Requesting {0} failed with code {1}", request.RequestUri.AbsoluteUri, resp.StatusCode.ToString());

                                LogException(e, message);

                                response = string.Empty;
                            }
                        }
                    }
                }
            }
            
            return response;
        }

        public static async Task<string> DownloadWebsiteAsStringAsync(HttpWebRequest request)
        {
            if (request == null) { throw new ArgumentNullException(nameof(request)); }

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
                                string message = string.Format(CultureInfo.CurrentCulture, "Requesting {0} failed with code {1}", request.RequestUri.AbsoluteUri, resp.StatusCode.ToString());

                                LogException(e, message);
                                
                                response = string.Empty;
                            }
                        }
                    }
                }
            }
            
            return response;
        }
    }
}