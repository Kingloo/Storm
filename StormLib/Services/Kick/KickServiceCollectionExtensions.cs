using System;
using System.Net;
using System.Net.Http;
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
			httpClient.BaseAddress = new Uri("https://kick.com/", UriKind.Absolute);
			httpClient.DefaultRequestVersion = HttpVersion.Version20;
			httpClient.Timeout = TimeSpan.FromSeconds(5d);
		}

		private static HttpMessageHandler ConfigurePrimaryHttpMessageHandler(IServiceProvider _)
		{
			return Helpers.HttpMessageHandlerHelpers.CreateDefaultHttpMessageHandler();
		}
	}
}