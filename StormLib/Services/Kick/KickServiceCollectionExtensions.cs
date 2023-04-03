using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StormLib.Services.Kick
{
	public static class KickServiceCollectionExtensions
	{
		public static IServiceCollection AddKick(this IServiceCollection services, IConfiguration configuration)
		{
			ArgumentNullException.ThrowIfNull(configuration);

			services.Configure<KickOptions>(configuration.GetSection("Kick"));

			services.AddHttpClient<KickUpdater>(HttpClientNames.Kick)
				.ConfigureHttpClient(ConfigureHttpClient)
				.ConfigurePrimaryHttpMessageHandler(ConfigurePrimaryHttpMessageHandler);
			
			services.AddTransient<KickUpdater>();

			return services;
		}

		private static void ConfigureHttpClient(IServiceProvider _, HttpClient httpClient)
		{
			Helpers.HttpClientHelpers.ConfigureDefaultHttpClient(httpClient);
		}

		private static HttpMessageHandler ConfigurePrimaryHttpMessageHandler(IServiceProvider _)
		{
			return new SocketsHttpHandler
			{
				AllowAutoRedirect = true,
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
				MaxAutomaticRedirections = 3,
				MaxConnectionsPerServer = 10,
				SslOptions = new SslClientAuthenticationOptions
				{
					AllowRenegotiation = false,
					ApplicationProtocols = new List<SslApplicationProtocol> { SslApplicationProtocol.Http2 },
#pragma warning disable CA5398
					EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
#pragma warning restore CA5398
					EncryptionPolicy = EncryptionPolicy.RequireEncryption
				}
			};
		}
	}
}