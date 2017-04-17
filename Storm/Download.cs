using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Threading.Tasks;
using Storm.Extensions;
using static System.FormattableString;

namespace Storm
{
    public enum DownloadResult
    {
        None = 0,
        Success = 1,
        Failure = 2,
        UriError = 3,
        FileError = 4,
        FileAlreadyExists = 5
    }

    public class DownloadEventArgs : EventArgs
    {
        private readonly int _totalBytes = 0;
        public int TotalBytes => _totalBytes;

        private readonly decimal _percent = 0m;
        public decimal Percent => _percent;

        public DownloadEventArgs(int totalBytes, decimal percent)
        {
            _totalBytes = totalBytes;
            _percent = percent;
        }
    }

    public class Download
    {
        public static async Task<string> WebsiteAsync(HttpWebRequest request)
        {
            if (request == null) { throw new ArgumentNullException(nameof(request)); }

            string website = string.Empty;
            
            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsyncExt();

            if (response == null)
            {
                request?.Abort();
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader sr
                        = new StreamReader(response.GetResponseStream()))
                    {
                        try
                        {
                            website = await sr.ReadToEndAsync()
                                .ConfigureAwait(false);
                        }
                        catch (IOException ex)
                        {
                            string message = Invariant($"Requesting {request.RequestUri.AbsoluteUri} failed: {response.StatusCode}");

                            await Log.LogExceptionAsync(ex, message)
                                .ConfigureAwait(false);
                        }
                        finally
                        {
                            response?.Dispose();
                        }
                    }
                }
            }

            return website;
        }

        public static async Task<string> WebsiteAsync(Uri uri)
        {
            if (uri == null) { throw new ArgumentNullException(nameof(uri)); }
            
            HttpWebRequest request = BuildStandardRequest(uri);

            return await WebsiteAsync(request)
                .ConfigureAwait(false);
        }

        private static HttpWebRequest BuildStandardRequest(Uri uri)
        {
            HttpWebRequest req = WebRequest.CreateHttp(uri);

            req.AllowAutoRedirect = true;
            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            req.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
            req.KeepAlive = false;
            req.Method = "GET";
            req.ProtocolVersion = HttpVersion.Version11;
            req.Referer = uri.DnsSafeHost;
            req.Timeout = 4000;
            req.UserAgent = ConfigurationManager.AppSettings["UserAgent"];

            req.Headers.Add("DNT", "1");
            req.Headers.Add("Accept-Encoding", "gzip, deflate");
            
            if (uri.Scheme.Equals("https"))
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }

            return req;
        }

        #region Properties
        private readonly Uri _uri = null;
        public Uri Uri => _uri;

        private readonly FileInfo _file = null;
        public FileInfo File => _file;

        private int _notifyEveryBytes = 1024 * 100; // 100 KiB default
        public int NotifyEveryBytes => _notifyEveryBytes;
        #endregion

        #region Events
        public event EventHandler<DownloadEventArgs> DownloadProgress;

        private void OnDownloadProgress(int totalBytes, decimal percent)
            => DownloadProgress?.Invoke(this, new DownloadEventArgs(totalBytes, percent));
        #endregion

        public Download(Uri uri, FileInfo file)
        {
            _uri = uri ?? throw new ArgumentNullException(nameof(uri));
            _file = file ?? throw new ArgumentNullException(nameof(file));
        }

        public Download(Uri uri, FileInfo file, int notifyEveryBytes)
            : this(uri, file)
        {
            _notifyEveryBytes = notifyEveryBytes;
        }

        public async Task<DownloadResult> ToFileAsync(bool deleteOnFailure)
        {
            if (_file.Exists) { return DownloadResult.FileAlreadyExists; }

            int memoryAndFileBuffer = 1024 * 1024 * 3; // 3 MiB

            var fsAsync = new FileStream(_file.FullName,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                memoryAndFileBuffer,
                FileOptions.Asynchronous);

            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)
            };

            HttpResponseMessage response = await client.GetAsync(
                _uri,
                HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode) { return DownloadResult.UriError; }
            
            int bytesRead = 0;
            int totalBytesRead = 0;
            int lastTotalBytesRead = 0;
            decimal length = Convert.ToDecimal(response.Content.Headers.ContentLength);
            decimal percent = 0m;
            byte[] buffer = new byte[memoryAndFileBuffer];

            Stream input = await response.Content.ReadAsStreamAsync()
                .ConfigureAwait(false);
            
            try
            {
                while ((bytesRead = await input
                .ReadAsync(buffer, 0, buffer.Length)
                .ConfigureAwait(false))
                > 0)
                {
                    percent = totalBytesRead / length;

                    totalBytesRead += bytesRead;

                    // without this limiter,
                    // the event would be invoked thousands of times unnecessarily
                    if (totalBytesRead > (lastTotalBytesRead + NotifyEveryBytes))
                    {
                        OnDownloadProgress(totalBytesRead, percent);

                        lastTotalBytesRead = totalBytesRead;
                    }
                    
                    await fsAsync.WriteAsync(buffer, 0, bytesRead)
                        .ConfigureAwait(false);
                }
            }
            catch (IOException)
            {
                if (deleteOnFailure && System.IO.File.Exists(_file.FullName))
                {
                    System.IO.File.Delete(_file.FullName);
                }

                return DownloadResult.Failure;
            }
            finally
            {
                if (fsAsync != null)
                {
                    await fsAsync.FlushAsync()
                        .ConfigureAwait(false);

                    fsAsync.Dispose();
                }
                
                client?.Dispose();
                response?.Dispose();
                input?.Dispose();
            }

            return DownloadResult.Success;
        }
    }
}
