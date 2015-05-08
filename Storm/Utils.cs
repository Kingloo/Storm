using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Storm
{
    public static class Utils
    {
        private static string logFilePath = string.Format(@"C:\Users\{0}\Documents\logfile.txt", Environment.UserName);
        private static int loggingRounds = 3;


        public static void SetWindowToMiddleOfScreen(Window window)
        {
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowHeight = window.Height;
            window.Top = (screenHeight / 2) - (windowHeight / 2);

            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double windowWidth = window.Width;
            window.Left = (screenWidth / 2) - (windowWidth / 2);
        }

        public static void SafeDispatcher(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            if (action == null) throw new ArgumentNullException("Utils.SafeDispatcher: action was null");

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

        public static async Task SafeDispatcherAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            if (action == null) throw new ArgumentNullException("Utils.SafeDispatcherAsync: action was null");

            Dispatcher disp = Application.Current.Dispatcher;

            if (disp.CheckAccess())
            {
                await Task.Run(() => action);
            }
            else
            {
                await disp.InvokeAsync(action, DispatcherPriority.Background);
            }
        }


        public static void OpenUriInBrowser(string uri)
        {
            Uri tmp = null;

            if (Uri.TryCreate(uri, UriKind.Absolute, out tmp))
            {
                Process.Start(tmp.AbsoluteUri);
            }
            else
            {
                string errorMessage = string.Format("Uri.TryCreate returned false on {0}", uri);

                Utils.LogMessage(errorMessage);
            }
        }

        public static void OpenUriInBrowser(Uri uri)
        {
            if (uri.IsAbsoluteUri)
            {
                Process.Start(uri.AbsoluteUri);
            }
            else
            {
                string errorMessage = string.Format("Uri ({0}) was not absolute", uri.OriginalString);

                Utils.LogMessage(errorMessage);
            }
        }


        public static void LogMessage(string message)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("{0} logged the following message at {1}", Process.GetCurrentProcess().MainModule.ModuleName, DateTime.Now));
            sb.AppendLine(message);
            sb.AppendLine(Environment.NewLine);

            WriteTextToFile(sb.ToString(), loggingRounds);
        }

        public static async Task LogMessageAsync(string message)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("{0} logged the following message at {1}", Process.GetCurrentProcess().MainModule.ModuleName, DateTime.Now));
            sb.AppendLine(message);
            sb.AppendLine(Environment.NewLine);

            await WriteTextToFileAsync(sb.ToString(), loggingRounds).ConfigureAwait(false);
        }


        public static void LogException(Exception e)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("{0} occurred in {1} at {2}", e.GetType().ToString(), Process.GetCurrentProcess().MainModule.ModuleName, DateTime.Now));

            sb.AppendLine(e.Message);
            sb.AppendLine(e.StackTrace);
            sb.AppendLine(Environment.NewLine);

            WriteTextToFile(sb.ToString(), loggingRounds);
        }

        public static void LogException(Exception e, string message)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("{0} occurred in {1} at {2}", e.GetType().ToString(), Process.GetCurrentProcess().MainModule.ModuleName, DateTime.Now));
            sb.AppendLine(message);
            sb.AppendLine(e.Message);
            sb.AppendLine(e.StackTrace);
            sb.AppendLine(Environment.NewLine);

            WriteTextToFile(sb.ToString(), loggingRounds);
        }

        public static async Task LogExceptionAsync(Exception e)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("{0} occurred in {1} at {2}", e.GetType().ToString(), Process.GetCurrentProcess().MainModule.ModuleName, DateTime.Now));
            sb.AppendLine(e.Message);
            sb.AppendLine(e.StackTrace);
            sb.AppendLine(Environment.NewLine);

            await WriteTextToFileAsync(sb.ToString(), loggingRounds).ConfigureAwait(false);
        }

        public static async Task LogExceptionAsync(Exception e, string message)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("{0} occurred in {1} at {2}", e.GetType().ToString(), Process.GetCurrentProcess().MainModule.ModuleName, DateTime.Now));
            sb.AppendLine(message);
            sb.AppendLine(e.Message);
            sb.AppendLine(e.StackTrace);
            sb.AppendLine(Environment.NewLine);

            await WriteTextToFileAsync(sb.ToString(), loggingRounds).ConfigureAwait(false);
        }


        private static void WriteTextToFile(string text, int rounds = 1)
        {
            if (rounds < 1) throw new ArgumentException("WriteTextToFile: rounds cannot be < 1");

            bool tryAgain = false;

            FileStream fs = null;
            
            try
            {
                fs = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.None, 1024, false);
            }
            catch (IOException)
            {
                tryAgain = (rounds > 1);

                if (fs != null)
                {
                    fs.Close();
                }
            }

            if (tryAgain)
            {
                int variation = DateTime.UtcNow.Millisecond;

                /*
                 * we want the delay to increase as the number of attempts left decreases
                 * as rounds increases, (1 / rounds) decreases
                 * => as (1 / rounds) decreases, (150 / (1 / rounds)) increases 
                 * 
                 * we convert rounds to decimal because otherwise it would do integer division
                 * e.g. 1 / 3 = 0
                 */
                decimal fixedWait = 150 / (1 / Convert.ToDecimal(rounds));

                int toWait = Convert.ToInt32(fixedWait) + variation;

                Thread.Sleep(toWait);

                WriteTextToFile(text, rounds - 1);
            }
            else
            {
                using (fs)
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.Write(text);
                    }
                }
            }
        }

        private static async Task WriteTextToFileAsync(string text, int rounds = 1)
        {
            if (rounds < 1) throw new ArgumentException("WriteTextToFile: rounds cannot be < 1");

            bool tryAgain = false;

            FileStream fsAsync = null;

            try
            {
                fsAsync = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.None, 1024, true);
            }
            catch (IOException)
            {
                tryAgain = (rounds > 1);

                if (fsAsync != null)
                {
                    fsAsync.Close();
                }
            }

            if (tryAgain)
            {
                int variation = DateTime.UtcNow.Millisecond;

                /*
                 * we want the delay to increase as the number of attempts left decreases
                 * as rounds increases, (1 / rounds) decreases
                 * => as (1 / rounds) decreases, (150 / (1 / rounds)) increases 
                 * 
                 * we convert rounds to decimal because otherwise it would do integer division
                 * e.g. 1 / 3 = 0
                 */
                decimal fixedWait = 150 / (1 / Convert.ToDecimal(rounds));

                int toWait = Convert.ToInt32(fixedWait) + variation;

                await Task.Delay(toWait).ConfigureAwait(false);

                await WriteTextToFileAsync(text, rounds - 1).ConfigureAwait(false);
            }
            else
            {
                using (fsAsync)
                {
                    using (StreamWriter sw = new StreamWriter(fsAsync))
                    {
                        await sw.WriteAsync(text).ConfigureAwait(false);
                    }
                }
            }
        }


        public static string DownloadWebsiteAsString(HttpWebRequest req)
        {
            string response = string.Empty;

            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponseExt())
            {
                if (resp == null)
                {
                    if (req != null)
                    {
                        req.Abort();
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
                            catch (IOException)
                            {
                                response = string.Empty;
                            }
                        }
                    }
                    else
                    {
                        string errorMessage = string.Format("Getting website {0} failed with code {1}", req.RequestUri.AbsoluteUri, resp.StatusCode.ToString());

                        Utils.LogMessage(errorMessage);
                    }
                }
            }

            return response;
        }

        public static async Task<string> DownloadWebsiteAsStringAsync(HttpWebRequest req)
        {
            string response = string.Empty;

            using (HttpWebResponse resp = (HttpWebResponse)(await req.GetResponseAsyncExt().ConfigureAwait(false)))
            {
                if (resp == null)
                {
                    if (req != null)
                    {
                        req.Abort();
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
                                response = await sr.ReadToEndAsync().ConfigureAwait(false);
                            }
                            catch (IOException)
                            {
                                response = string.Empty;
                            }
                        }
                    }
                    else
                    {
                        string errorMessage = string.Format("Getting website {0} failed with code: {1}, desc: {2}", req.RequestUri.AbsoluteUri, resp.StatusCode.ToString(), resp.StatusDescription);

                        await Utils.LogMessageAsync(errorMessage).ConfigureAwait(false);
                    }
                }
            }

            return response;
        }
    }
}