using System;
using System.IO;

namespace StormLib.Services.Kick
{
	public class KickStream : BaseStream
	{
		private Uri? _icon = null;
		public override Uri Icon
		{
			get
			{
				if (_icon is null)
				{
					string path = Path.Combine(IconDirectory, "Kick.ico");

					_icon = new Uri(path);
				}

				return _icon;
			}
		}

		public override bool HasStreamlinkSupport { get => false; }

		public override string ServiceName { get => "Kick"; }

		private string? sessionTitle = null;
		public string? SessionTitle
		{
			get => sessionTitle;
			set => SetProperty(ref sessionTitle, value, nameof(SessionTitle));
		}

		public KickStream(Uri uri)
			: base(uri)
		{ }
	}
}
