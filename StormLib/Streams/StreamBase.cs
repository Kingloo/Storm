using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using StormLib.Common;
using StormLib.Interfaces;

namespace StormLib.Streams
{
	public abstract class StreamBase : BindableBase, IStream
	{
		public Uri Link { get; }

		private Status _status = Status.None;
		public Status Status
		{
			get => _status;
			set => SetProperty(ref _status, value, nameof(Status));
		}

		private string _name = "unset Name";
		public string Name
		{
			get => _name;
			set => SetProperty(ref _name, value, nameof(Name));
		}

		private string _displayName = string.Empty;
		public string DisplayName
		{
			get => String.IsNullOrWhiteSpace(_displayName) ? _name : _displayName;
			set => SetProperty(ref _displayName, value, nameof(DisplayName));
		}

		private Nullable<int> _viewersCount = null;
		public Nullable<int> ViewersCount
		{
			get => _viewersCount;
			set => SetProperty(ref _viewersCount, value, nameof(ViewersCount));
		}

        protected static string IconDirectory
        {
            get
            {
                return Path.Combine(RuntimeCircumstance.GetRealLocation(), "Icons");
            }
        }

		public abstract Uri Icon { get; }
		public abstract bool HasStreamlinkSupport { get; }
		public abstract string ServiceName { get; }

		protected StreamBase(Uri uri)
		{
			if (uri is null)
			{
				throw new ArgumentNullException(nameof(uri));
			}

			Link = uri;

#pragma warning disable CA2214
			_name = DetermineName(Link);
#pragma warning restore CA2214
		}

		protected virtual string DetermineName(Uri uri)
		{
			if (uri is null)
			{
				throw new ArgumentNullException(nameof(uri));
			}

			return uri
				.Segments
				.FirstOrDefault(s => s != "/")
				?.TrimEnd(Char.Parse("/"))
			?? uri.AbsoluteUri;
		}

		public bool Equals(IStream? other) => (other is StreamBase sb) && EqualsInternal(sb);

		public override bool Equals(object? obj) => (obj is StreamBase sb) && EqualsInternal(sb);

		public static bool operator ==(StreamBase lhs, StreamBase rhs)
		{
			if (lhs is null && rhs is null)
			{
				return true;
			}

			if (lhs is null)
			{
				return false;
			}

			if (rhs is null)
			{
				return false;
			}

			return lhs.Equals(rhs);
		}

		public static bool operator !=(StreamBase lhs, StreamBase rhs)
		{
			if (lhs is null && rhs is null)
			{
				return false;
			}

			if (lhs is null)
			{
				return true;
			}

			if (rhs is null)
			{
				return true;
			}

			return !lhs.Equals(rhs);
		}

		private bool EqualsInternal(StreamBase other) => Link.Equals(other.Link);

		public virtual int CompareTo(IStream? other) => String.Compare(Name, other?.Name ?? string.Empty, StringComparison.Ordinal);

		public static bool operator >(StreamBase lhs, StreamBase rhs)
		{
			if (lhs is null && rhs is null)
			{
				return false;
			}

			if (lhs is null)
			{
				return false;
			}

			if (rhs is null)
			{
				return true;
			}

			return lhs.CompareTo(rhs) > 0;
		}

		public static bool operator >=(StreamBase lhs, StreamBase rhs)
		{
			if (lhs is null && rhs is null)
			{
				return true;
			}

			if (lhs is null)
			{
				return false;
			}

			if (rhs is null)
			{
				return true;
			}

			return lhs.CompareTo(rhs) >= 0;
		}

		public static bool operator <(StreamBase lhs, StreamBase rhs)
		{
			if (lhs is null && rhs is null)
			{
				return false;
			}

			if (lhs is null)
			{
				return true;
			}

			if (rhs is null)
			{
				return false;
			}

			return lhs.CompareTo(rhs) < 0;
		}

		public static bool operator <=(StreamBase lhs, StreamBase rhs)
		{
			if (lhs is null && rhs is null)
			{
				return true;
			}

			if (lhs is null)
			{
				return true;
			}

			if (rhs is null)
			{
				return false;
			}

			return lhs.CompareTo(rhs) <= 0;
		}

		public override int GetHashCode() => Link.GetHashCode();

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine(base.ToString());
			sb.AppendLine(CultureInfo.CurrentCulture, $"link: {Link.AbsoluteUri}");
			sb.AppendLine(CultureInfo.CurrentCulture, $"status: {Status}");
			sb.AppendLine(CultureInfo.CurrentCulture, $"name: {Name}");
			sb.AppendLine(CultureInfo.CurrentCulture, $"display name: {DisplayName}");
			sb.AppendLine(CultureInfo.CurrentCulture, $"viewers: {ViewersCount}");
			sb.AppendLine(CultureInfo.CurrentCulture, $"icon path: {Icon.AbsoluteUri}");
			sb.AppendLine(CultureInfo.CurrentCulture, $"has streamlink support: {HasStreamlinkSupport}");
			sb.AppendLine(CultureInfo.CurrentCulture, $"service name: {ServiceName}");

			return sb.ToString();
		}
	}
}
