using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FileLogger;
using StormDesktop.Interfaces;
using StormLib;
using StormLib.Services.Chaturbate;
using StormLib.Services.Kick;
using StormLib.Services.Mixlr;
using StormLib.Services.Rumble;
using StormLib.Services.Twitch;
using StormLib.Services.YouTube;

namespace StormDesktop.Gui
{
	public partial class App : Application
	{
		private readonly IHost host;
		private readonly ILogger<App> logger;

		public App()
		{
			InitializeComponent();

			host = BuildHost();

			logger = host.Services.GetRequiredService<ILogger<App>>();
		}

		private static IHost BuildHost()
		{
			return new HostBuilder()
				.ConfigureHostConfiguration(ConfigureHostConfiguration)
				.ConfigureHostOptions(ConfigureHostOptions)
				.ConfigureAppConfiguration(ConfigureAppConfiguration)
				.ConfigureServices(ConfigureServices)
				.Build();
		}

		private static void ConfigureHostConfiguration(IConfigurationBuilder configurationBuilder)
		{
			configurationBuilder
				.AddCommandLine(Environment.GetCommandLineArgs())
				.AddEnvironmentVariables();
		}

		private static void ConfigureHostOptions(HostOptions hostOptions)
		{
			hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
		}

		private static void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder configurationBuilder)
		{
			if (!context.HostingEnvironment.IsProduction())
			{
				string environmentMessage = $"environment is {context.HostingEnvironment.EnvironmentName}";

				Console.Out.WriteLine(environmentMessage);
				Console.Error.WriteLine(environmentMessage);
				Debug.WriteLineIf(Debugger.IsAttached, environmentMessage);
			}

			const string permanent = "appsettings.json";
			string environment = $"appsettings.{context.HostingEnvironment.EnvironmentName}.json";

			configurationBuilder
				.AddJsonFile(permanent, optional: false, reloadOnChange: true)
				.AddJsonFile(environment, optional: true, reloadOnChange: true);
		}

		private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
		{
			services.AddLogging((ILoggingBuilder loggingBuilder) =>
			{
				loggingBuilder.AddConfiguration(context.Configuration.GetSection("Logging"));

				loggingBuilder.AddDebug();

				loggingBuilder.AddFileLogger();
			});

			services.AddSingleton<IFileLoggerSink, FileLoggerSink>();

			services.Configure<StormOptions>(context.Configuration.GetSection("Storm"));

			services.AddChaturbate(context.Configuration);
			services.AddKick(context.Configuration);
			services.AddMixlr(context.Configuration);
			services.AddRumble(context.Configuration);
			services.AddTwitch(context.Configuration);
			services.AddYouTube(context.Configuration);

			services.AddSingleton<UpdaterMessageQueue>();

			services.AddHostedService<
				StormBackgroundService<
					ChaturbateStream,
					ChaturbateUpdater,
					IOptionsMonitor<ChaturbateOptions>,
					ChaturbateOptions>>();

			services.AddHostedService<
				StormBackgroundService<
					KickStream,
					KickUpdater,
					IOptionsMonitor<KickOptions>,
					KickOptions>>();

			services.AddHostedService<
				StormBackgroundService<
					MixlrStream,
					MixlrUpdater,
					IOptionsMonitor<MixlrOptions>,
					MixlrOptions>>();

			services.AddHostedService<
				StormBackgroundService<
					RumbleStream,
					RumbleUpdater,
					IOptionsMonitor<RumbleOptions>,
					RumbleOptions>>();

			services.AddHostedService<
				StormBackgroundService<
					TwitchStream,
					TwitchUpdater,
					IOptionsMonitor<TwitchOptions>,
					TwitchOptions>>();

			services.AddHostedService<
				StormBackgroundService<
					YouTubeStream,
					YouTubeUpdater,
					IOptionsMonitor<YouTubeOptions>,
					YouTubeOptions>>();

			services.AddTransient<IMainWindowViewModel, MainWindowViewModel>();
			services.AddTransient<MainWindow>();
		}

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			logger.LogDebug("app startup started");

			host.Start();

			IHostApplicationLifetime appLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
			appLifetime.ApplicationStopping.Register(Shutdown);

			IFileLoggerSink sink = host.Services.GetRequiredService<IFileLoggerSink>();

			sink.StartSink();

			MainWindow = host.Services.GetRequiredService<MainWindow>();

			MainWindow.Show();

			logger.LogInformation("app started");
		}

		private void Application_Exit(object? sender, ExitEventArgs e)
		{
			logger.LogDebug("app exit started");

			IFileLoggerSink sink = host.Services.GetRequiredService<IFileLoggerSink>();

			sink.StopSink();

			host.StopAsync().GetAwaiter().GetResult();

			logger.LogInformation("app exited");

			host.Dispose();
		}

		private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			if (e.Exception is Exception ex)
			{
				Exception toLog = ex;

				if (ex is TargetInvocationException tie && tie.InnerException is not null)
				{
					toLog = tie.InnerException;
				}

				logger.LogError(toLog, "an unhandled exception occurred in WinGui ({FullName} from {Source})", toLog.GetType().FullName, toLog.Source);
				logger.LogDebug("StackTrace of {FullName}{NewLine}{StackTrace}", toLog.GetType().FullName, Environment.NewLine, ex.StackTrace);
			}
			else
			{
				logger.LogError("an EMPTY unhandled exception occurred in WinGui");
			}
		}
	}
}
