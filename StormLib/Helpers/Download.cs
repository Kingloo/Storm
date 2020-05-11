using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using StormLib.Interfaces;

namespace StormLib.Helpers
{
    public class Download : IDownload
    {
        private readonly HttpMessageHandler handler;
        private readonly HttpClient client;
        private readonly TimeSpan defaultTimeout = TimeSpan.FromSeconds(5d);

        public bool IsActive { get; private set; } = false;

        public Download()
        {
            handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                MaxAutomaticRedirections = 3
            };

            client = new HttpClient(handler, false)
            {
                Timeout = defaultTimeout
            };
        }

        public Download(HttpMessageHandler handler)
        {
            this.handler = handler;

            client = new HttpClient(this.handler, false)
            {
                Timeout = defaultTimeout
            };
        }

        public Task<(HttpStatusCode, string)> StringAsync(Uri uri)
            => StringAsync(new HttpRequestMessage(HttpMethod.Get, uri));

        public async Task<(HttpStatusCode, string)> StringAsync(HttpRequestMessage request)
        {
            IsActive = true;

            HttpStatusCode status = HttpStatusCode.Unused;
            string text = string.Empty;

            HttpResponseMessage? response = null;

            try
            {
                response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
            catch (HttpRequestException) { }
            catch (TaskCanceledException) { }
            finally
            {
                request.Dispose();

                if (!(response is null))
                {
                    status = response.StatusCode;

                    response.Dispose();
                }
            }

            IsActive = false;

            return (status, text);
        }

        public Task<(HttpStatusCode, byte[])> DataAsync(Uri uri) => throw new NotImplementedException();

        public Task<(HttpStatusCode, byte[])> DataAsync(HttpRequestMessage request) => throw new NotImplementedException();

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    client.Dispose();
                    handler.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
