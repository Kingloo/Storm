using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Storm.Common;
using Storm.Model;

namespace Storm.DataAccess
{
    public class TxtRepo
    {
        private FileInfo _urlsFile = null;
        public FileInfo UrlsFile => _urlsFile;
        
        public TxtRepo(FileInfo urlsFile)
        {
            _urlsFile = urlsFile;
        }
        
        public async Task<IEnumerable<StreamBase>> LoadAsync()
        {
            var streams = new List<StreamBase>();

            FileStream fsAsync = null;

            try
            {
                fsAsync = new FileStream(
                    UrlsFile.FullName,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.None,
                    4096,
                    FileOptions.Asynchronous | FileOptions.SequentialScan);

                using (StreamReader sr = new StreamReader(fsAsync))
                {
                    fsAsync = null;

                    string line = string.Empty;

                    while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        if (line.StartsWith("#", StringComparison.CurrentCultureIgnoreCase))
                        {
                            continue;
                        }

                        if (ParseIntoStream(line) is StreamBase sb)
                        {
                            streams.Add(sb);
                        }
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Log.LogException(ex);
            }
            finally
            {
                fsAsync?.Dispose();
            }

            return streams;
        }

        private static StreamBase ParseIntoStream(string line)
        {
            // if this is not done subsequent Uri.TryCreate will fail
            if (line.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase) == false
                && line.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase) == false)
            {
                line = string.Concat("http://", line);
            }
            
            if (!Uri.TryCreate(line, UriKind.Absolute, out Uri uri))
            {
                return new UnsupportedService("invalid Uri");
            }
            
            return DetermineStreamingService(uri);
        }
        
        private static StreamBase DetermineStreamingService(Uri uri)
        {
            string dnsSafe = uri.DnsSafeHost;

            var sc = StringComparison.OrdinalIgnoreCase;

            if (dnsSafe.EndsWith("twitch.tv", sc)) { return new Twitch(uri); }
            if (dnsSafe.EndsWith("ustream.tv", sc)) { return new Ustream(uri); }
            if (dnsSafe.EndsWith("mixlr.com", sc)) { return new Mixlr(uri); }
            if (dnsSafe.EndsWith("hitbox.tv", sc)) { return new Hitbox(uri); }
            if (dnsSafe.EndsWith("beam.pro", sc)) { return new Beam(uri); }
            if (dnsSafe.EndsWith("mixer.com", sc)) { return new Mixer(uri); }
            if (dnsSafe.EndsWith("chaturbate.com", sc)) { return new Chaturbate(uri); }
            if (dnsSafe.EndsWith("youtube.com", sc)) { return new YouTube(uri); }
            
            return new UnsupportedService(uri.AbsoluteUri);
        }
    }
}
