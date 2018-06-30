using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Storm.Common
{
    public enum DownloadResult
    {
        None,
        Success,
        Failure,
        Canceled,
        FileAlreadyExists,
        WebError
    }

    public class DownloadProgress
    {
        private readonly Int64 _bytesRead = 0L;
        public Int64 BytesRead => _bytesRead;

        private readonly Int64 _totalBytesReceived = 0L;
        public Int64 TotalBytesReceived => _totalBytesReceived;

        private readonly Int64? _contentLength = null;
        public Int64? ContentLength => _contentLength;

        public DownloadProgress(Int64 bytesRead, Int64 totalBytesReceived)
            : this(bytesRead, totalBytesReceived, null)
        { }

        public DownloadProgress(Int64 bytesRead, Int64 totalBytesReceived, Int64? contentLength)
        {
            _bytesRead = bytesRead;
            _totalBytesReceived = totalBytesReceived;
            _contentLength = contentLength;
        }
    }

    public class Download
    {
        private const string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:59.0) Gecko/20100101 Firefox/59.0";

        private static HttpClientHandler handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            MaxAutomaticRedirections = 3
        };

        private static HttpClient client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(5d)
        };


        #region Properties
        private readonly Uri _uri = default;
        public Uri Uri => _uri;

        private readonly FileInfo _file = default;
        public FileInfo File => _file;
        #endregion

        public Download(Uri uri, FileInfo file)
        {
            _uri = uri ?? throw new ArgumentNullException(nameof(uri));
            _file = file ?? throw new ArgumentNullException(nameof(file));
        }

        public Task<DownloadResult> ToFileAsync(IProgress<DownloadProgress> progress) => ToFileAsync(progress, CancellationToken.None);

        public async Task<DownloadResult> ToFileAsync(IProgress<DownloadProgress> progress, CancellationToken token)
        {
            if (File.Exists) { return DownloadResult.FileAlreadyExists; }

            var request = new HttpRequestMessage(HttpMethod.Get, Uri);
            request.Headers.Add("User-Agent", userAgent);

            Stream receive = null;
            Stream save = null;

            Int64? contentLength = null;

            using (HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false))
            {
                receive = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                save = new FileStream(File.FullName, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous);

                contentLength = response.Content.Headers.ContentLength;
            }

            int bytesRead = 0;
            Int64 totalBytesReceived = 0L;

            byte[] buffer = new byte[1024 * 100]; // 100 KiB

            try
            {
                while ((bytesRead = await receive.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
                {
                    totalBytesReceived += bytesRead;

                    if (contentLength.HasValue)
                    {
                        progress.Report(new DownloadProgress(bytesRead, totalBytesReceived, contentLength));
                    }
                    else
                    {
                        progress.Report(new DownloadProgress(bytesRead, totalBytesReceived));
                    }

                    await save.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
                }

                await save.FlushAsync().ConfigureAwait(false);
            }
            catch (HttpRequestException)
            {
                return DownloadResult.WebError;
            }
            catch (IOException)
            {
                return DownloadResult.Failure;
            }
            catch (TaskCanceledException)
            {
                return DownloadResult.Canceled;
            }
            finally
            {
                request?.Dispose();
                receive?.Dispose();
                save?.Dispose();
            }

            return DownloadResult.Success;
        }


        public static Task<string> WebsiteAsync(Uri uri) => WebsiteAsync(uri, CancellationToken.None);

        public static Task<string> WebsiteAsync(Uri uri, CancellationToken token)
        {
            if (uri == null) { throw new ArgumentNullException(nameof(uri)); }

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("User-Agent", userAgent);

            return DownloadStringAsync(request, token);
        }

        public static Task<string> WebsiteAsync(HttpRequestMessage request) => WebsiteAsync(request, CancellationToken.None);

        public static Task<string> WebsiteAsync(HttpRequestMessage request, CancellationToken token)
        {
            if (request == null) { throw new ArgumentNullException(nameof(request)); }

            return DownloadStringAsync(request, token);
        }


        private static async Task<string> DownloadStringAsync(HttpRequestMessage request, CancellationToken token)
        {
            string text = string.Empty;

            try
            {
                using (HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        text = await Task.Run(response.Content.ReadAsStringAsync, token).ConfigureAwait(false);
                    }
                    else
                    {
                        string message = string.Format(
                            CultureInfo.CurrentCulture,
                            "downloading {0}: {1}",
                            request.RequestUri.AbsoluteUri,
                            response.StatusCode);

                        await Log.LogMessageAsync(message).ConfigureAwait(false);
                    }
                }
            }
            catch (HttpRequestException) { }
            catch (TaskCanceledException) { }
            finally
            {
                request?.Dispose();
            }

            return text;
        }        
    }
}
