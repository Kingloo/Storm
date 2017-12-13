using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Storm.Common
{
    public static class Utils
    {
        public static void DispatchSafely(Dispatcher dispatcher, Action action)
        {
            DispatchSafely(dispatcher, action, DispatcherPriority.Normal);
        }

        public static void DispatchSafely(Dispatcher dispatcher, Action action, DispatcherPriority priority)
        {
            if (dispatcher == null) { throw new ArgumentNullException(nameof(dispatcher)); }
            if (action == null) { throw new ArgumentNullException(nameof(action)); }

            if (dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                dispatcher.Invoke(action, priority);
            }
        }
        
        public static async Task DispatchSafelyAsync(Dispatcher dispatcher, Action action)
        {
            await DispatchSafelyAsync(dispatcher, action, DispatcherPriority.Normal);
        }

        public static async Task DispatchSafelyAsync(Dispatcher dispatcher, Action action, DispatcherPriority priority)
        {
            if (dispatcher == null) { throw new ArgumentNullException(nameof(dispatcher)); }
            if (action == null) { throw new ArgumentNullException(nameof(action)); }
            
            if (dispatcher.CheckAccess())
            {
                await Task.Run(action);
            }
            else
            {
                await dispatcher.InvokeAsync(action, priority);
            }
        }

        
        public static void OpenUriInBrowser(Uri uri)
        {
            if (uri == null) { throw new ArgumentNullException(nameof(uri)); }

            if (uri.IsAbsoluteUri)
            {
                Process.Start(uri.AbsoluteUri);
            }
            else
            {
                string errorMessage = $"Uri ({uri.OriginalString}) was not absolute";

                Log.LogMessage(errorMessage);
            }
        }

        public static void OpenUriInBrowser(string link)
        {
            if (String.IsNullOrWhiteSpace(link))
            {
                throw new ArgumentNullException(nameof(link));
            }
            
            if (Uri.TryCreate(link, UriKind.Absolute, out Uri uri))
            {
                OpenUriInBrowser(uri);
            }
            else
            {
                string errorMessage = $"Uri.TryCreate returned false on {uri}";

                Log.LogMessage(errorMessage);
            }
        }
    }
}