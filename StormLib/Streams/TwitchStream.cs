using System;

namespace StormLib.Streams
{
    public class TwitchStream : StreamBase
    {
        public override bool HasStreamlinkSupport => true;

        public override string ServiceName => "Twitch";

        private string _game = string.Empty;
        public string Game
        {
            get => _game;
            set => SetProperty(ref _game, value, nameof(Game));
        }

        public TwitchStream(Uri uri)
            : base(uri)
        { }
    }
}