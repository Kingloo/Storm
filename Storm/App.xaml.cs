using System.Net;
using System.Windows;
using System.Windows.Threading;
using Storm.DataAccess;

namespace Storm
{
    public partial class App : Application
    {
        private readonly IRepository _urlsRepo = null;
        public IRepository UrlsRepo { get { return _urlsRepo; } }

        public App(IRepository repo)
        {
            _urlsRepo = repo;

            // without this it will reuse the same connection for a given service
            // e.g. checking 10 twitch accounts will use the same connection for all 10 tries
            // in SEQUENCE, bad
            // increase this to allow them to occur in parallel
            // improves performance significantly

            ServicePointManager.DefaultConnectionLimit = 10;
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Utils.LogException(e.Exception);
        }
    }
}
