using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Storm.Wpf.StreamServices
{
    public static class Helpers
    {
        private static readonly HttpClient client = new HttpClient();

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

        public static Task<(bool, string)> DownloadStringAsync(Uri uri) => DownloadStringAsync(uri, null);

        public static async Task<(bool, string)> DownloadStringAsync(Uri uri, Action<HttpRequestMessage> configureHeaders)
        {
            if (uri is null) { throw new ArgumentNullException(nameof(uri)); }

            bool success = false;
            string text = string.Empty;

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            configureHeaders?.Invoke(request);

            try
            {
                var httpOption = HttpCompletionOption.ResponseHeadersRead;

                using (var response = await client.SendAsync(request, httpOption).ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                        success = true;
                    }
                }
            }
            catch (Exception) { }
            finally
            {
                request.Dispose();
            }

            return (success, text);
        }
    }
}
