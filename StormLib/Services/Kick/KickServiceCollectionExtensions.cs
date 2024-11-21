using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace StormLib.Services.Kick
{
	public static class KickServiceCollectionExtensions
	{
		/*
			Exercise extreme caution when changing Kick's HttpClient or HttpMessageHandler settings !!!
		*/

		public static IServiceCollection AddKick(this IServiceCollection services, IConfiguration configuration)
		{
			ArgumentNullException.ThrowIfNull(configuration);

			services.Configure<KickOptions>(configuration.GetSection("Kick"));

			services.AddTransient<KickUpdater>(CreateKickUpdater);

			return services;
		}

		private static KickUpdater CreateKickUpdater(IServiceProvider serviceProvider)
		{
			ILogger<KickUpdater> logger = serviceProvider.GetRequiredService<ILogger<KickUpdater>>();

#pragma warning disable CA2000 // see HttpClient->ctor disposeHandler is true
			SocketsHttpHandler handler = ConfigurePrimaryHttpMessageHandler(serviceProvider);
#pragma warning restore CA2000

			HttpClient httpClient = new HttpClient(handler, disposeHandler: true);

			ConfigureHttpClient(httpClient);

			IOptionsMonitor<KickOptions> kickOptionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<KickOptions>>();
			IOptionsMonitor<StormOptions> stormOptionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<StormOptions>>();

			return new KickUpdater(logger, httpClient, kickOptionsMonitor, stormOptionsMonitor);
		}

		private static void ConfigureHttpClient(HttpClient client)
		{
			client.DefaultRequestVersion = HttpVersion.Version20;
			client.Timeout = TimeSpan.FromSeconds(60d);
		}

		private static SocketsHttpHandler ConfigurePrimaryHttpMessageHandler(IServiceProvider _)
		{
			return new SocketsHttpHandler
			{
				AllowAutoRedirect = false,
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
				CookieContainer = new CookieContainer(),
				MaxConnectionsPerServer = 1,
				SslOptions = new SslClientAuthenticationOptions
				{
					AllowRenegotiation = false,
#pragma warning disable CA5398 // I want to hardcode this
					EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
#pragma warning restore CA5398
					EncryptionPolicy = EncryptionPolicy.RequireEncryption
				},
				UseCookies = true
			};
		}
	}
}
