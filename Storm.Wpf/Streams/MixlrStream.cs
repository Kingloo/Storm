using System;

namespace Storm.Wpf.Streams
{
    public class MixlrStream : StreamBase
    {
        private static readonly Uri _icon = new Uri($"{IconPackUriPrefix}Mixlr.ico");
        public override Uri Icon => _icon;

        public MixlrStream(Uri account)
            : base(account)
        { }
    }
}
