using System;
using System.IO;

namespace StormLib.Streams
{
    public class UnsupportedStream : StreamBase
    {
        private Uri? _icon = null;
        public override Uri Icon
        {
            get
            {
                if (_icon is null)
                {
                    string path = Path.Combine(iconDirectory, "Unsupported.ico");

                    _icon = new Uri(path);
                }

                return _icon;
            }
        }

        public override bool HasStreamlinkSupport => true;

        public override string ServiceName => "Unsupported";

        public UnsupportedStream(Uri uri)
            : base(uri)
        { }
    }
}