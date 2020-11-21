using System;
using System.IO;
using System.Linq;

namespace StormLib.Streams
{
    public class YouTubeStream : StreamBase
    {
        private Uri? _icon = null;
        public override Uri Icon
        {
            get
            {
                if (_icon is null)
                {
                    string path = Path.Combine(IconDirectory, "YouTube.ico");

                    _icon = new Uri(path);
                }

                return _icon;
            }
        }

        public override bool HasStreamlinkSupport => true;

        public override string ServiceName => "YouTube";

        public YouTubeStream(Uri uri)
            : base(uri)
        { }

        protected override string DetermineName(Uri uri)
        {
            return uri.Segments.LastOrDefault(s => s != "/")?.TrimEnd(Char.Parse("/")) ?? uri.AbsoluteUri;
        }
    }
}
