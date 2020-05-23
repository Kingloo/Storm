using System;

namespace StormLib.Streams
{
    public class MixerStream : StreamBase
    {
        public override bool HasStreamlinkSupport => true;

        public override string ServiceName => "Mixer";

        private string _game = string.Empty;
        public string Game
        {
            get => _game;
            set => SetProperty(ref _game, value, nameof(Game));
        }

        public MixerStream(Uri uri)
            : base(uri)
        { }
    }
}