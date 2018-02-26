using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Storm.Common
{
    public static class Log
    {
        private static FileInfo logFile = GetLogFile();

        private static FileInfo GetLogFile()
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException(nameof(directory));
            }

            string filename = "logfile.txt";
            
            string fullPath = Path.Combine(directory, filename);

            return File.Exists(fullPath) ? new FileInfo(fullPath) : CreateLogFile(fullPath);
        }

        private static FileInfo CreateLogFile(string fullPath)
        {
            using (StreamWriter sw = File.CreateText(fullPath))
            {
                return new FileInfo(fullPath);
            }
        }


        public static void LogMessage(string message)
        {
            string text = FormatMessage(message);

            WriteToFile(text);
        }

        public static async Task LogMessageAsync(string message)
        {
            string text = FormatMessage(message);

            await WriteToFileAsync(text).ConfigureAwait(false);
        }


        public static void LogException(Exception ex)
            => LogException(ex, string.Empty, false);

        public static void LogException(Exception ex, string message)
            => LogException(ex, message, false);

        public static void LogException(Exception ex, bool includeStackTrace)
            => LogException(ex, string.Empty, includeStackTrace);

        public static void LogException(Exception ex, string message, bool includeStackTrace)
        {
            if (ex == null) { return; }

            StringBuilder sb = new StringBuilder();

            if (String.IsNullOrWhiteSpace(message))
            {
                sb.AppendLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "{0} - {1}",
                        ex.GetType().FullName,
                        ex.Message));
            }
            else
            {
                sb.AppendLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "{0} - {1} - {2}",
                        ex.GetType().FullName,
                        ex.Message,
                        message));
            }

            if (includeStackTrace)
            {
                sb.AppendLine(ex.StackTrace);
            }

            LogMessage(sb.ToString());
        }

        public static async Task LogExceptionAsync(Exception ex)
            => await LogExceptionAsync(ex, string.Empty, false).ConfigureAwait(false);

        public static async Task LogExceptionAsync(Exception ex, string message)
            => await LogExceptionAsync(ex, message, false).ConfigureAwait(false);

        public static async Task LogExceptionAsync(Exception ex, bool includeStackTrace)
            => await LogExceptionAsync(ex, string.Empty, includeStackTrace).ConfigureAwait(false);

        public static async Task LogExceptionAsync(Exception ex, string message, bool includeStackTrace)
        {
            if (ex == null) { return; }

            StringBuilder sb = new StringBuilder();
            
            if (String.IsNullOrWhiteSpace(message))
            {
                sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "{0} - {1}", ex.GetType().FullName, ex.Message));
            }
            else
            {
                sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "{0} - {1} - {2}", ex.GetType().FullName, ex.Message, message));
            }
            
            if (includeStackTrace)
            {
                sb.AppendLine(ex.StackTrace);
            }
            
            await LogMessageAsync(sb.ToString()).ConfigureAwait(false);
        }


        private static string FormatMessage(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss (zzz)", CultureInfo.CurrentCulture);
            string processName = Process.GetCurrentProcess().MainModule.ModuleName;

            return string.Format(CultureInfo.CurrentCulture, "{0} - {1} - {2}", timestamp, processName, message);
        }


        private static void WriteToFile(string text)
        {
            FileStream fs = null;

            try
            {
                fs = new FileStream(
                    logFile.FullName,
                    FileMode.Append,
                    FileAccess.Write,
                    FileShare.None,
                    4096,
                    FileOptions.None);

                using (StreamWriter sw = new StreamWriter(fs))
                {
                    fs = null;

                    sw.Write(text);
                }
            }
            catch (FileNotFoundException) { }
            catch (IOException) { }
            finally
            {
                fs?.Close();
            }
        }

        private static async Task WriteToFileAsync(string text)
        {
            FileStream fsAsync = null;

            try
            {
                fsAsync = new FileStream(
                    logFile.FullName,
                    FileMode.Append,
                    FileAccess.Write,
                    FileShare.None,
                    4096,
                    FileOptions.Asynchronous);

                using (StreamWriter sw = new StreamWriter(fsAsync))
                {
                    fsAsync = null;

                    await sw.WriteAsync(text).ConfigureAwait(false);
                }
            }
            catch (FileNotFoundException) { }
            catch (IOException) { }
            finally
            {
                fsAsync?.Close();
            }
        }
    }
}
