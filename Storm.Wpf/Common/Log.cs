using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Storm.Wpf.Common
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


        public static void Message(string message)
        {
            string text = FormatMessage(message);

            WriteToFile(text);
        }

        public static Task MessageAsync(string message)
        {
            string text = FormatMessage(message);

            return WriteToFileAsync(text);
        }


        public static void Exception(Exception ex)
            => Exception(ex, string.Empty, false);

        public static void Exception(Exception ex, string message)
            => Exception(ex, message, false);

        public static void Exception(Exception ex, bool includeStackTrace)
            => Exception(ex, string.Empty, includeStackTrace);

        public static void Exception(Exception ex, string message, bool includeStackTrace)
        {
            if (ex is null) { throw new ArgumentNullException(nameof(ex)); }

            string text = FormatException(ex, message, includeStackTrace);

            Message(text);
        }

        public static Task ExceptionAsync(Exception ex)
            => ExceptionAsync(ex, string.Empty, false);

        public static Task ExceptionAsync(Exception ex, string message)
            => ExceptionAsync(ex, message, false);

        public static Task ExceptionAsync(Exception ex, bool includeStackTrace)
            => ExceptionAsync(ex, string.Empty, includeStackTrace);

        public static Task ExceptionAsync(Exception ex, string message, bool includeStackTrace)
        {
            if (ex is null) { throw new ArgumentNullException(nameof(ex)); }

            string text = FormatException(ex, message, includeStackTrace);
            
            return MessageAsync(text);
        }


        private static string FormatMessage(string message)
        {
            var cc = CultureInfo.CurrentCulture;

            string timestamp = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss (zzz)", cc);
            string processName = Process.GetCurrentProcess().MainModule.ModuleName;

            return string.Format(cc, "{0} - {1} - {2}", timestamp, processName, message);
        }

        private static string FormatException(Exception ex, string message, bool includeStackTrace)
        {
            var sb = new StringBuilder();

            sb.Append(ex.GetType().FullName);
            sb.Append(" - ");
            sb.Append(ex.Message);

            if (!String.IsNullOrWhiteSpace(message))
            {
                sb.Append(" - ");
                sb.Append(message);
            }
            
            if (includeStackTrace)
            {
                sb.AppendLine(ex.StackTrace);
            }

            return sb.ToString();
        }


        private static void WriteToFile(string text)
        {
            FileStream fs = default;

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

                    sw.WriteLine(text);

                    sw.Flush();
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
            FileStream fsAsync = default;

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

                    await sw.WriteLineAsync(text).ConfigureAwait(false);

                    await sw.FlushAsync().ConfigureAwait(false);
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
