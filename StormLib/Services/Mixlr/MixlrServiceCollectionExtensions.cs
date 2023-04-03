using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StormLib.Services.Mixlr
{
	public static class MixlrServiceCollectionExtensions
	{
		public static IServiceCollection AddMixlr(this IServiceCollection services, IConfiguration configuration)
		{
			ArgumentNullException.ThrowIfNull(configuration);

			services.Configure<MixlrOptions>(configuration.GetSection("Mixlr"));

			services.AddHttpClient<MixlrUpdater>(HttpClientNames.Mixlr)
				.ConfigureHttpClient(ConfigureHttpClient)
				.ConfigurePrimaryHttpMessageHandler(ConfigurePrimaryHttpMessageHandler);
			
			services.AddTransient<MixlrUpdater>();

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