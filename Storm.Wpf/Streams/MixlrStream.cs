using System;

namespace Storm.Wpf.Streams
{
    public class MixlrStream : StreamBase
    {
        protected override string ServiceName { get; } = "Mixlr";

        private static readonly Uri _icon = new Uri($"{IconPackPrefix}Mixlr.ico");
        public override Uri Icon => _icon;

        public MixlrStream(Uri account)
            : base(account)
        { }
    }
}
