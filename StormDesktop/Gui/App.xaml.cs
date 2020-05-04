using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;
using System.Windows;
using System.Windows.Threading;
using StormDesktop.Common;
using StormDesktop.Interfaces;
using StormLib;
using StormLib.Helpers;
using StormLib.Interfaces;

namespace StormDesktop.Gui
{
    public partial class App : Application
    {
        private static readonly string defaultLogFileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static readonly string defaultLogFileName = "logfile.txt";
        private static readonly string defaultLogFilePath = Path.Combine(defaultLogFileDirectory, defaultLogFileName);

        private static readonly string defaultStreamsFileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static readonly string defaultStreamsFileName = "StormUrls.txt";
        private static readonly string defaultStreamsFilePath = Path.Combine(defaultStreamsFileDirectory, defaultStreamsFileName);

        private readonly string filePath = string.Empty;

        public App()
            : this(defaultStreamsFilePath)
        { }

        public App(string filePath)
        {
            this.filePath = filePath;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ILogClass logger = new LogClass(defaultLogFilePath, Severity.Warning);

            SocketsHttpHandler handler = new SocketsHttpHandler
            {
                AllowAutoRedirect = true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
                MaxAutomaticRedirections = 3,
                MaxConnectionsPerServer = 10,
                SslOptions = new SslClientAuthenticationOptions
                {
                    AllowRenegotiation = false,
                    ApplicationProtocols = new List<SslApplicationProtocol> { SslApplicationProtocol.Http2 },
                    EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                    EncryptionPolicy = EncryptionPolicy.RequireEncryption
                }
            };

            IDownload downloader = new Download(handler);

            IServicesManager servicesManager = new ServicesManager(downloader);

            servicesManager.AddDefaultServices();

            IMainWindowViewModel viewModel = new MainWindowViewModel(logger, servicesManager, filePath);

            MainWindow = new MainWindow(viewModel);

            MainWindow.Show();
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is Exception ex)
            {
                LogStatic.Exception(ex);
            }
            else
            {
                string message = string.Format(CultureInfo.CurrentCulture, "an empty {0} was thrown", nameof(DispatcherUnhandledException));

                LogStatic.Message(message, Severity.Error);
            }
        }
    }
}
