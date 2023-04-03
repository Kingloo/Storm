using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StormLib.Services.Twitch
{
	public static class TwitchServiceCollectionExtensions
	{
		public static IServiceCollection AddTwitch(this IServiceCollection services, IConfiguration configuration)
		{
			ArgumentNullException.ThrowIfNull(configuration);

			services.Configure<TwitchOptions>(configuration.GetSection("Twitch"));

			services.AddHttpClient<TwitchUpdater>(HttpClientNames.Twitch)
				.ConfigureHttpClient(ConfigureHttpClient)
				.ConfigurePrimaryHttpMessageHandler(ConfigurePrimaryHttpMessageHandler);
			
			services.AddTransient<TwitchUpdater>();

			return services;
		}

		private static void ConfigureHttpClient(IServiceProvider _, HttpClient httpClient)
		{
			Helpers.HttpClientHelpers.ConfigureDefaultHttpClient(httpClient);
		}

		private static HttpMessageHandler ConfigurePrimaryHttpMessageHandler(IServiceProvider _)
		{
			return Helpers.HttpMessageHandlerHelpers.CreateDefaultHttpMessageHandler();
		}
	}
}