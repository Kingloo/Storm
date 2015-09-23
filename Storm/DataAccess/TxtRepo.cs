using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Storm.DataAccess
{
    class TxtRepo : IRepository
    {
        private string _filePath = string.Empty;
        public string FilePath
        {
            get
            {
                return _filePath;
            }
            set
            {
                _filePath = value;
            }
        }

        public TxtRepo(string filePath)
        {
            if (String.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("filePath was null or whitespace");

            _filePath = filePath;
        }

        public async Task<IEnumerable<StreamBase>> LoadAsync()
        {
            List<StreamBase> streams = null;

            try
            {
                using (FileStream fsAsync = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.None, 2048, true))
                using (StreamReader sr = new StreamReader(fsAsync))
                {
                    string fileAsString = await sr.ReadToEndAsync().ConfigureAwait(false);

                    streams = await ParseStringAsync(fileAsString).ConfigureAwait(false);
                }
            }
            catch (FileNotFoundException e)
            {
                Utils.LogException(e);
            }

            return streams != null ? streams : Enumerable.Empty<StreamBase>();
        }

        private async Task<List<StreamBase>> ParseStringAsync(string fileAsString)
        {
            List<StreamBase> toReturn = new List<StreamBase>();

            StringReader sr = new StringReader(fileAsString);

            string line = string.Empty;

            while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                if (line.StartsWith("#") == false)
                {
                    if (line.StartsWith("http://") == false && line.StartsWith("https://") == false)
                    {
                        line = string.Concat("http://", line);
                    }

                    Uri tmp = null;

                    if (Uri.TryCreate(line, UriKind.Absolute, out tmp))
                    {
                        StreamBase sb = DetermineStreamingService(tmp);

                        toReturn.Add(sb);
                    }
                }
            }

            return toReturn;
        }

        private StreamBase DetermineStreamingService(Uri tmp)
        {
            StreamBase sb = null;

            switch (tmp.DnsSafeHost)
            {
                case "twitch.tv":
                    sb = new Twitch(tmp);
                    break;
                case "ustream.tv":
                    sb = new Ustream(tmp);
                    break;
                default:
                    sb = new UnsupportedService(tmp.AbsoluteUri);
                    break;
            }

            return sb;
        }

        public Task SaveAsync(IEnumerable<StreamBase> streams)
        {
            throw new NotImplementedException("editing of urls to be done through invoking notepad.exe");
        }
    }
}
