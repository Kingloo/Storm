using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StormLib.Services.Chaturbate
{
	public static class ChaturbateServiceCollectionExtensions
	{
		public static IServiceCollection AddChaturbate(this IServiceCollection services, IConfiguration configuration)
		{
			// services.Configure<ChaturbateOptions>(configuration.GetSection("Chaturbate"));

			services.AddHttpClient<ChaturbateUpdater>(HttpClientNames.Chaturbate)
				.ConfigureHttpClient(ConfigureHttpClient)
				.ConfigurePrimaryHttpMessageHandler(ConfigurePrimaryHttpMessageHandler);
			
			// services.AddTransient<IUpdater<ChaturbateUpdater>, ChaturbateUpdater>();

			return services;
		}

		private static void ConfigureHttpClient(IServiceProvider _, HttpClient httpClient)
		{
			httpClient.BaseAddress = new Uri("https://chaturbate.com/", UriKind.Absolute);
			httpClient.DefaultRequestVersion = HttpVersion.Version20;
			httpClient.Timeout = TimeSpan.FromSeconds(5d);
		}

		private static HttpMessageHandler ConfigurePrimaryHttpMessageHandler(IServiceProvider _)
		{
			return Helpers.HttpMessageHandlerHelpers.CreateDefaultHttpMessageHandler();
		}
	}
}