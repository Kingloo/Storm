using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StormLib.Services.Chaturbate
{
	public static class ChaturbateServiceCollectionExtensions
	{
		public static IServiceCollection AddChaturbate(this IServiceCollection services, IConfiguration configuration)
		{
			ArgumentNullException.ThrowIfNull(configuration);

			services.Configure<ChaturbateOptions>(configuration.GetSection("Chaturbate"));

			services.AddHttpClient<ChaturbateUpdater>(HttpClientNames.Chaturbate)
				.ConfigureHttpClient(ConfigureHttpClient)
				.ConfigurePrimaryHttpMessageHandler(ConfigurePrimaryHttpMessageHandler);

			services.AddTransient<ChaturbateUpdater>();

			return services;
		}

		private static void ConfigureHttpClient(IServiceProvider _, HttpClient httpClient)
		{
			httpClient.BaseAddress = new Uri("https://chaturbate.com/", UriKind.Absolute);

			Helpers.HttpClientHelpers.ConfigureDefaultHttpClient(httpClient);
		}

		private static HttpMessageHandler ConfigurePrimaryHttpMessageHandler(IServiceProvider _)
		{
			return Helpers.HttpMessageHandlerHelpers.CreateDefaultHttpMessageHandler();
		}
	}
}
