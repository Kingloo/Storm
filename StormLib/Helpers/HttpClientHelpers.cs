using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace StormLib.Helpers
{
	internal static class HttpClientHelpers
	{
		internal static void ConfigureDefaultHttpClient(HttpClient client)
		{
			client.DefaultRequestVersion = HttpVersion.Version20;
			client.Timeout = TimeSpan.FromSeconds(60d);
		}

		internal static ValueTask<(HttpStatusCode, string)> GetStringAsync(HttpClient client, Uri uri)
			=> GetStringAsync(client, uri, null, CancellationToken.None);

		internal static ValueTask<(HttpStatusCode, string)> GetStringAsync(HttpClient client, Uri uri, CancellationToken cancellationToken)
			=> GetStringAsync(client, uri, null, cancellationToken);

		internal static async ValueTask<(HttpStatusCode, string)> GetStringAsync(HttpClient client, Uri uri, Action<HttpRequestMessage>? configureRequestMessage, CancellationToken cancellationToken)
		{
			(HttpStatusCode, string) response = (HttpStatusCode.Unused, string.Empty);
			
			using (HttpRequestMessage requestMessage = new HttpRequestMessage
			{
				RequestUri = uri
			})
			{
				configureRequestMessage?.Invoke(requestMessage);

				response = await GetStringAsync(client, requestMessage, cancellationToken).ConfigureAwait(false);
			}

			return response;
		}

		internal static async ValueTask<(HttpStatusCode, string)> GetStringAsync(HttpClient client, HttpRequestMessage requestMessage, CancellationToken cancellationToken)
		{
			HttpStatusCode statusCode = HttpStatusCode.Unused;
			string text = string.Empty;

			HttpResponseMessage? responseMessage = null;
			
			try
			{
				responseMessage = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

				text = await responseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
			}
			finally
			{
				if (responseMessage is not null)
				{
					statusCode = responseMessage.StatusCode;

					responseMessage.Dispose();
				}
			}

			return (statusCode, text);
		}
	}
}