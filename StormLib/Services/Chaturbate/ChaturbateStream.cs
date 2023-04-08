using System;
using System.IO;

namespace StormLib.Services.Chaturbate
{
	public class ChaturbateStream : BaseStream
	{
		private Uri? _icon = null;
		public override Uri Icon
		{
			get
			{
				if (_icon is null)
				{
					string path = Path.Combine(IconDirectory, "Chaturbate.ico");

					_icon = new Uri(path);
				}

				return _icon;
			}
		}

		public override bool HasStreamlinkSupport { get => true; }

		public override string ServiceName { get => "Chaturbate"; }

		public ChaturbateStream(Uri uri)
			: base(uri)
		{ }
	}
}
