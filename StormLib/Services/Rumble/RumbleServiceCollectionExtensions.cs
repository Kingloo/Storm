using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StormLib.Services.Rumble
{
	public static class RumbleServiceCollectionExtensions
	{
		public static IServiceCollection AddRumble(this IServiceCollection services, IConfiguration configuration)
		{
			ArgumentNullException.ThrowIfNull(configuration);

			services.Configure<RumbleOptions>(configuration.GetSection("Rumble"));

			services.AddHttpClient<RumbleUpdater>(HttpClientNames.Rumble)
				.ConfigureHttpClient(ConfigureHttpClient)
				.ConfigurePrimaryHttpMessageHandler(ConfigurePrimaryHttpMessageHandler);

			services.AddTransient<RumbleUpdater>();

			return services;
		}

		private static void ConfigureHttpClient(IServiceProvider _, HttpClient httpClient)
		{
			httpClient.BaseAddress = new Uri("https://rumble.com/", UriKind.Absolute);

			Helpers.HttpClientHelpers.ConfigureDefaultHttpClient(httpClient);
		}

		private static HttpMessageHandler ConfigurePrimaryHttpMessageHandler(IServiceProvider _)
		{
			return Helpers.HttpMessageHandlerHelpers.CreateDefaultHttpMessageHandler();
		}
	}
}
