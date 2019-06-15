using System;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace Storm.Wpf.Common
{
    public static class Web
    {
        private static readonly HttpClientHandler handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
            MaxAutomaticRedirections = 3,
            SslProtocols = SslProtocols.Tls12
        };

        private static readonly HttpClient client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(7d)
        };

        public static Task<(HttpStatusCode, string)> DownloadStringAsync(Uri uri)
            => DownloadStringAsync(uri, null);

        public static async Task<(HttpStatusCode, string)> DownloadStringAsync(Uri uri, Action<HttpRequestMessage> configureRequest)
        {
            if (uri is null) { throw new ArgumentNullException(nameof(uri)); }

            HttpStatusCode status = HttpStatusCode.Unused;
            string html = string.Empty;

            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            configureRequest?.Invoke(request);

            try
            {
                using (var response = await client.SendAsync(request).ConfigureAwait(false))
                {
                    html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    status = response.StatusCode;
                }
            }
            catch (HttpRequestException) { }
            finally
            {
                request?.Dispose();
            }

            return (status, html);
        }
    }
}
