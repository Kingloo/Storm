using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace StormLib.Helpers
{
	internal sealed record HttpResponse(HttpStatusCode StatusCode, string Response);

	internal static class HttpClientHelpers
	{
		internal static void ConfigureDefaultHttpClient(HttpClient client)
		{
			client.DefaultRequestVersion = HttpVersion.Version20;
			client.Timeout = TimeSpan.FromSeconds(60d);
		}

		internal static ValueTask<HttpResponse> GetStringAsync(HttpClient client, Uri uri)
			=> GetStringAsyncInternal(client, uri, null, CancellationToken.None);

		internal static ValueTask<HttpResponse> GetStringAsync(HttpClient client, Uri uri, CancellationToken cancellationToken)
			=> GetStringAsyncInternal(client, uri, null, cancellationToken);

		internal static ValueTask<HttpResponse> GetStringAsync(HttpClient client, Uri uri, Action<HttpRequestMessage> configureRequestMessage, CancellationToken cancellationToken)
			=> GetStringAsyncInternal(client, uri, configureRequestMessage, cancellationToken);

		private static async ValueTask<HttpResponse> GetStringAsyncInternal(HttpClient client, Uri uri, Action<HttpRequestMessage>? configureRequestMessage, CancellationToken cancellationToken)
		{
			HttpResponse response;

			using HttpRequestMessage requestMessage = new HttpRequestMessage
			{
				RequestUri = uri
			};
			configureRequestMessage?.Invoke(requestMessage);

			response = await GetStringAsync(client, requestMessage, cancellationToken).ConfigureAwait(false);

			return response with { Response = WebUtility.HtmlDecode(response.Response) };
		}

		private static async ValueTask<HttpResponse> GetStringAsync(HttpClient client, HttpRequestMessage requestMessage, CancellationToken cancellationToken)
		{
			HttpStatusCode statusCode = HttpStatusCode.Unused;
			string text = string.Empty;

			HttpResponseMessage? responseMessage = null;

			try
			{
				responseMessage = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

				text = await responseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
			}
			catch (TimeoutException) { }
			catch (HttpRequestException) { }
			catch (SocketException) { }
			finally
			{
				if (responseMessage is not null)
				{
					statusCode = responseMessage.StatusCode;

					responseMessage.Dispose();
				}
			}

			return new HttpResponse(statusCode, text);
		}
	}
}
