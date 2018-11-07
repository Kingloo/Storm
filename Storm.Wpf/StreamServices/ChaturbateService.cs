using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Storm.Wpf.Common;
using Storm.Wpf.Streams;
using static Storm.Wpf.StreamServices.Helpers;

namespace Storm.Wpf.StreamServices
{
    public class ChaturbateService
    {
        private const string offline = "Room is currently offline";
        private const string login = "meta name=\"keywords\" content=\"Login, Chaturbate login\"";
        private const string banned = "has been banned";

        public static async Task UpdateAsync(IEnumerable<ChaturbateStream> streams)
        {
            if (streams is null) { throw new ArgumentNullException(nameof(streams)); }
            if (!streams.Any()) { return; }

            foreach (ChaturbateStream stream in streams)
            {
                stream.IsLive = await GetIsLiveAsync(stream.AccountLink);
            }
        }

        private static async Task<bool> GetIsLiveAsync(Uri uri)
        {
            (bool success, string page) = await DownloadStringAsync(uri).ConfigureAwait(false);

            if (!success) { return false; }
            // if downloading the page fails we declare them offline no matter what

            using (StringReader sr = new StringReader(page))
            {
                string line = string.Empty;

                while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (line.Contains(offline)
                        || line.Contains(login))
                    {
                        return false;
                    }
                    else if (line.Contains(banned))
                    {
                        string message = $"{uri.AbsoluteUri} was banned";

                        await Log.LogMessageAsync(message).ConfigureAwait(false);

                        return false;
                    }
                }
            }

            return true;
        }
    }
}
