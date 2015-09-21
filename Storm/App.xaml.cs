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
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Utils.LogException(e.Exception);
        }
    }
}
