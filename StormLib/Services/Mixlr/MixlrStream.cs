using System;
using System.IO;

namespace StormLib.Services.Mixlr
{
	public class MixlrStream : BaseStream
	{
		private Uri? _icon = null;
		public override Uri Icon
		{
			get
			{
				if (_icon is null)
				{
					string path = Path.Combine(IconDirectory, "Mixlr.ico");

					_icon = new Uri(path);
				}

				return _icon;
			}
		}

		public override bool HasStreamlinkSupport { get => true; }

		public override string ServiceName { get => "Mixlr"; }

		public MixlrStream(Uri uri)
			: base(uri)
		{ }
	}
}
