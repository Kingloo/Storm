using System;

namespace StormLib.Streams
{
    public class MixlrStream : StreamBase
    {
        public override bool HasStreamlinkSupport => true;

        public override string ServiceName => "Mixlr";

        public MixlrStream(Uri uri)
            : base(uri)
        { }
    }
}