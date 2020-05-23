using System;

namespace StormLib.Streams
{
    public class ChaturbateStream : StreamBase
    {
        public override bool HasStreamlinkSupport => true;

        public override string ServiceName => "Chaturbate";

        public ChaturbateStream(Uri uri)
            : base(uri)
        { }
    }
}