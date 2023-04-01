using System;
using System.IO;
using StormLib.Services;

namespace StormLib.Streams
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

		public override bool HasStreamlinkSupport => true;

		public override string ServiceName => "Mixlr";

		public MixlrStream(Uri uri)
			: base(uri)
		{ }
	}
}
