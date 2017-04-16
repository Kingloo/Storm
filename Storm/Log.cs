using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Storm
{
    public static class Log
    {
        private static int rounds = 5;
        private static string logFilePath = GetLogFilePath();

        private static string GetLogFilePath()
        {
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filename = "logfile.txt";

            return Path.Combine(dir, filename);
        }

        public static void LogMessage(string message)
        {
            if (message == null) { throw new ArgumentNullException(nameof(message)); }

            WriteTextToFile(message, rounds);
        }

        public static async Task LogMessageAsync(string message)
        {
            if (message == null) { throw new ArgumentNullException(nameof(message)); }

            await WriteTextToFileAsync(message, rounds)
                .ConfigureAwait(false);
        }


        public static void LogException(Exception ex)
        {
            LogException(ex, string.Empty);
        }

        public static void LogException(Exception ex, string message)
        {
            if (ex == null) { throw new ArgumentNullException(nameof(ex)); }
            if (message == null) { throw new ArgumentNullException(nameof(message)); }

            StringBuilder sb = new StringBuilder();

            if (!String.IsNullOrWhiteSpace(message))
            {
                sb.AppendLine(message);
            }

            sb.AppendLine(ex.Message);
            sb.AppendLine(ex.StackTrace);

            WriteTextToFile(sb.ToString(), rounds);
        }

        public static async Task LogExceptionAsync(Exception ex)
        {
            await LogExceptionAsync(ex)
                .ConfigureAwait(false);
        }

        public static async Task LogExceptionAsync(Exception ex, string message)
        {
            if (ex == null) { throw new ArgumentNullException(nameof(ex)); }
            if (message == null) { throw new ArgumentNullException(nameof(message)); }

            StringBuilder sb = new StringBuilder();

            if (!String.IsNullOrWhiteSpace(message))
            {
                sb.AppendLine(message);
            }

            sb.AppendLine(ex.Message);
            sb.AppendLine(ex.StackTrace);

            await WriteTextToFileAsync(sb.ToString(), rounds)
                .ConfigureAwait(false);
        }


        private static string PrepareLogMessage(string text)
        {
            StringBuilder sb = new StringBuilder();

            DateTime time = DateTime.Now;
            string name = Process.GetCurrentProcess().MainModule.ModuleName;

            string log = $"{time} - {name}";

            sb.AppendLine(log);
            sb.AppendLine(text);

            return sb.ToString();
        }

        private static void WriteTextToFile(string text, int rounds = 1)
        {
            if (text == null) { throw new ArgumentNullException(nameof(text)); }
            if (rounds < 1) { throw new ArgumentException("WriteTextToFile: rounds cannot be < 1"); }

            string log = PrepareLogMessage(text);

            bool tryAgain = false;

            FileStream fs = null;

            try
            {
                fs = new FileStream(logFilePath,
                    FileMode.Append,
                    FileAccess.Write,
                    FileShare.None,
                    4096,
                    false);

                using (StreamWriter sw = new StreamWriter(fs))
                {
                    fs = null;

                    sw.WriteLine(log);
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

                WriteTextToFile(log, rounds - 1);
            }
        }

        private static async Task WriteTextToFileAsync(string text, int rounds = 1)
        {
            if (text == null) { throw new ArgumentNullException(nameof(text)); }
            if (rounds < 1) { throw new ArgumentException("WriteTextToFileAsync: rounds cannot be < 1"); }

            string log = PrepareLogMessage(text);

            bool tryAgain = false;

            FileStream fsAsync = null;

            try
            {
                fsAsync = new FileStream(logFilePath,
                    FileMode.Append,
                    FileAccess.Write,
                    FileShare.None,
                    4096,
                    true);

                using (StreamWriter sw = new StreamWriter(fsAsync))
                {
                    fsAsync = null;

                    await sw.WriteLineAsync(log)
                        .ConfigureAwait(false);
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

                await Task.Delay(toWait)
                    .ConfigureAwait(false);

                await WriteTextToFileAsync(log, rounds - 1)
                    .ConfigureAwait(false);
            }
        }
    }
}
