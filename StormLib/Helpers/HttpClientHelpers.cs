using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace StormLib.Helpers
{
	internal static class HttpClientHelpers
	{
		internal static ValueTask<(HttpStatusCode, string)> GetStringAsync(HttpClient client, Uri uri)
			=> GetStringAsync(client, uri, CancellationToken.None);

		internal static ValueTask<(HttpStatusCode, string)> GetStringAsync(HttpClient client, Uri uri, CancellationToken cancellationToken)
		{
			using HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
			
			return GetStringAsync(client, requestMessage, cancellationToken);
		}

		internal static ValueTask<(HttpStatusCode, string)> GetStringAsync(HttpClient client, Uri uri, Action<HttpRequestMessage> configureRequestMessage, CancellationToken cancellationToken)
		{
			using HttpRequestMessage requestMessage = new HttpRequestMessage
			{
				RequestUri = uri
			};

			configureRequestMessage.Invoke(requestMessage);

			return GetStringAsync(client, requestMessage, cancellationToken);
		}

		internal static async ValueTask<(HttpStatusCode, string)> GetStringAsync(HttpClient client, HttpRequestMessage requestMessage, CancellationToken cancellationToken)
		{
			HttpStatusCode statusCode = HttpStatusCode.Unused;
			string text = string.Empty;

			HttpResponseMessage? responseMessage = null;
			
			try
			{
				responseMessage = await client.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

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