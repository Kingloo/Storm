using System;
using System.IO;
using System.Linq;

namespace StormLib.Services.YouTube
{
	public class YouTubeStream : BaseStream
	{
		private Uri? _icon = null;
		public override Uri Icon
		{
			get
			{
				if (_icon is null)
				{
					string path = Path.Combine(IconDirectory, "YouTube.ico");

					_icon = new Uri(path);
				}

				return _icon;
			}
		}

		public override bool HasStreamlinkSupport { get => true; }

		public override string ServiceName { get => "YouTube"; }

		public YouTubeStream(Uri uri)
			: base(uri)
		{ }

		protected override string DetermineName(Uri uri)
		{
			ArgumentNullException.ThrowIfNull(uri);

			return uri.Segments.LastOrDefault(s => s != "/")?.TrimEnd(Char.Parse("/")) ?? uri.AbsoluteUri;
		}
	}
}
