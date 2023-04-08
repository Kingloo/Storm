using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StormLib.Services.YouTube
{
	public static class YouTubeServiceCollectionExtensions
	{
		public static IServiceCollection AddYouTube(this IServiceCollection services, IConfiguration configuration)
		{
			ArgumentNullException.ThrowIfNull(configuration);

			services.Configure<YouTubeOptions>(configuration.GetSection("YouTube"));

			services.AddHttpClient<YouTubeUpdater>(HttpClientNames.YouTube)
				.ConfigureHttpClient(ConfigureHttpClient)
				.ConfigurePrimaryHttpMessageHandler(ConfigurePrimaryHttpMessageHandler);

			services.AddTransient<YouTubeUpdater>();

			return services;
		}

		private static void ConfigureHttpClient(IServiceProvider _, HttpClient httpClient)
		{
			httpClient.BaseAddress = new Uri("https://youtube.com/", UriKind.Absolute);

			Helpers.HttpClientHelpers.ConfigureDefaultHttpClient(httpClient);
		}

		private static HttpMessageHandler ConfigurePrimaryHttpMessageHandler(IServiceProvider _)
		{
			return Helpers.HttpMessageHandlerHelpers.CreateDefaultHttpMessageHandler();
		}
	}
}
