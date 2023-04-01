using System;
using System.IO;

namespace StormLib.Services
{
	public class UnsupportedStream : BaseStream
	{
		private Uri? _icon = null;
		public override Uri Icon
		{
			get
			{
				if (_icon is null)
				{
					string path = Path.Combine(IconDirectory, "Unsupported.ico");

					_icon = new Uri(path);
				}

				return _icon;
			}
		}

		public override bool HasStreamlinkSupport { get => false; }

		public override string ServiceName { get => "Unsupported"; }

		public UnsupportedStream(Uri uri)
			: base(uri)
		{
			Status = Status.Unsupported;
		}
	}
}
