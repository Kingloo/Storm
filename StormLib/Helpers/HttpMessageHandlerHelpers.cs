using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;

namespace StormLib.Helpers
{
	internal static class HttpMessageHandlerHelpers
	{
		internal static HttpMessageHandler CreateDefaultHttpMessageHandler()
		{
			return new SocketsHttpHandler
			{
				AllowAutoRedirect = true,
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
				MaxAutomaticRedirections = 2,
				MaxConnectionsPerServer = 1,
				PooledConnectionLifetime = TimeSpan.FromMinutes(10d),
				SslOptions = new SslClientAuthenticationOptions
				{
					AllowRenegotiation = false,
					ApplicationProtocols = new List<SslApplicationProtocol> { SslApplicationProtocol.Http2 },
#pragma warning disable CA5398 // I don't want to let the OS choose. It might choose wrong.
					EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
#pragma warning restore CA5398
					EncryptionPolicy = EncryptionPolicy.RequireEncryption
				},
				UseCookies = false,
				UseProxy = false
			};
		}
	}
}
