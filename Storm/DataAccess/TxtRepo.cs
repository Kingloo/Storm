using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Storm.Model;

namespace Storm.DataAccess
{
    public class TxtRepo : IRepository
    {
        private string _filePath = string.Empty;
        public string FilePath { get { return _filePath; } }
        
        public TxtRepo(string filePath)
        {
            if (String.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("filePath was null or whitespace");
            }

            _filePath = filePath;
        }

        public void SetFilePath(string newPath)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<StreamBase>> LoadAsync()
        {
            List<StreamBase> streams = new List<StreamBase>();

            FileStream fsAsync = null;

            try
            {
                fsAsync = new FileStream(FilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.None,
                4096,
                true);

                using (StreamReader sr = new StreamReader(fsAsync))
                {
                    fsAsync = null;

                    string line = string.Empty;

                    while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        if (line.StartsWith("#", StringComparison.CurrentCultureIgnoreCase)) { continue; }

                        StreamBase sb = ParseIntoStream(line);

                        if (sb != null)
                        {
                            streams.Add(sb);
                        }
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                Utils.LogException(e);
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

            if (dnsSafe.Contains("twitch.tv")) { return new Twitch(uri); }
            if (dnsSafe.Contains("ustream.tv")) { return new Ustream(uri); }
            if (dnsSafe.Contains("mixlr.com")) { return new Mixlr(uri); }
            if (dnsSafe.Contains("hitbox.tv")) { return new Hitbox(uri); }
            if (dnsSafe.Contains("beam.pro")) { return new Beam(uri); }
            if (dnsSafe.Contains("chaturbate.com")) { return new Chaturbate(uri); }
            if (dnsSafe.Contains("youtube.com")) { return new YouTube(uri); }

            return new UnsupportedService(uri.AbsoluteUri);
        }

        public Task SaveAsync(IEnumerable<StreamBase> streams)
        {
            throw new NotImplementedException("editing of urls to be done through invoking notepad.exe");
        }
    }
}
