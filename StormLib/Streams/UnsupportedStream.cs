using System;

namespace StormLib.Streams
{
    public class UnsupportedStream : StreamBase
    {
        public override bool HasStreamlinkSupport => true;

        public override string ServiceName => "Unsupported";

        public UnsupportedStream(Uri uri)
            : base(uri)
        { }
    }
}