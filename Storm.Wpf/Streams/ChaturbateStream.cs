using System;

namespace Storm.Wpf.Streams
{
    public class ChaturbateStream : StreamBase
    {
        protected override string ServiceName { get; } = "Chaturbate";

        private static readonly Uri _icon = new Uri($"{IconPackPrefix}Chaturbate.ico");
        public override Uri Icon => _icon;

        public ChaturbateStream(Uri account)
            : base(account)
        { }
    }
}
