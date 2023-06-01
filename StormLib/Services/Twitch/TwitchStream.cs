using System;
using System.Globalization;
using System.IO;

namespace StormLib.Services.Twitch
{
	public class TwitchStream : BaseStream
	{
		private const string embeddedUriFormat = "https://player.twitch.tv/?branding=false&channel={0}&parent=twitch.tv&showInfo=false";

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

		public override bool HasStreamlinkSupport { get => true; }

		public override string ServiceName { get => "Twitch"; }

		private TwitchGame? game = null;
		public TwitchGame? Game
		{
			get => game;
			set => SetProperty(ref game, value, nameof(Game));
		}

		public TwitchStream(Uri uri)
			: base(uri)
		{ }

		public override Uri GetBrowserLink()
		{
			return new Uri(string.Format(CultureInfo.InvariantCulture, embeddedUriFormat, Name), UriKind.Absolute);
		}
	}
}
