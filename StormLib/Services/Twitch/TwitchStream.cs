using System;
using System.IO;

namespace StormLib.Streams
{
	public class TwitchStream : StreamBase
	{
		private Uri? _icon = null;
		public override Uri Icon
		{
			get
			{
				if (_icon is null)
				{
					string path = Path.Combine(IconDirectory, "Twitch.ico");

					_icon = new Uri(path);
				}

				return _icon;
			}
		}

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
