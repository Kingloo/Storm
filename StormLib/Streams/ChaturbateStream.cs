using System;
using System.IO;

namespace StormLib.Streams
{
    public class ChaturbateStream : StreamBase
    {
        private Uri? _icon = null;
        public override Uri Icon
        {
            get
            {
                if (_icon is null)
                {
                    string path = Path.Combine(iconDirectory, "Chaturbate.ico");

                    _icon = new Uri(path);
                }

                return _icon;
            }
        }

        public override bool HasStreamlinkSupport => true;

        public override string ServiceName => "Chaturbate";

        public ChaturbateStream(Uri uri)
            : base(uri)
        { }
    }
}