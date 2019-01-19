using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Storm.Wpf.StreamServices
{
    public static class Helpers
    {
        public static bool TryParseJson(string rawJson, out JObject json)
        {
            try
            {
                json = JObject.Parse(rawJson);
                return true;
            }
            catch (JsonReaderException)
            {
                json = null;
                return false;
            }
        }


        private static readonly HttpClient client = new HttpClient(handler);

        private static readonly HttpClientHandler handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
            MaxAutomaticRedirections = 3,
            SslProtocols = SslProtocols.Tls12
        };


        public static Task<(bool, string)> DownloadStringAsync(Uri uri) => DownloadStringAsync(uri, null);

        public static async Task<(bool, string)> DownloadStringAsync(Uri uri, Action<HttpRequestMessage> configureRequest)
        {
            if (uri is null) { throw new ArgumentNullException(nameof(uri)); }

            bool success = false;
            string text = string.Empty;

            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            configureRequest?.Invoke(request);

            var httpOption = HttpCompletionOption.ResponseHeadersRead;

            try
            {
                using (var response = await client.SendAsync(request, httpOption).ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                        success = true;
                    }
                }
            }
            catch (HttpRequestException)
            {
                success = false;
            }
            catch (IOException)
            {
                success = false;
            }
            catch (TaskCanceledException)
            {
                success = false;
            }
            finally
            {
                request.Dispose();
            }

            return (success, text);
        }
    }
}
