using System;
using System.IO;

namespace StormLib.Streams
{
	public class RumbleStream : StreamBase
	{
		private Uri? _icon = null;
		public override Uri Icon
		{
			get
			{
				if (_icon is null)
				{
					string path = Path.Combine(IconDirectory, "Rumble.ico");

					_icon = new Uri(path);
				}

				return _icon;
			}
		}

		public override bool HasStreamlinkSupport { get => false; }

		public override string ServiceName { get => "Rumble"; }

		public RumbleStream(Uri uri)
			: base(uri)
		{ }
	}
}