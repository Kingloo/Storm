using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using StormLib.Common;
using StormLib.Interfaces;

namespace StormLib.Services
{
	public abstract class BaseStream : BindableBase, IStream
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

		public virtual Uri GetBrowserLink()
		{
			return Link;
		}

		protected BaseStream(Uri uri)
		{
			ArgumentNullException.ThrowIfNull(uri);

			Link = uri;

#pragma warning disable CA2214
			_name = DetermineName(Link);
#pragma warning restore CA2214
		}

		protected virtual string DetermineName(Uri uri)
		{
			ArgumentNullException.ThrowIfNull(uri);

			return uri
				.Segments
				.FirstOrDefault(s => s != "/")
				?.TrimEnd(Char.Parse("/"))
			?? uri.AbsoluteUri;
		}

		public bool Equals(IStream? other) => (other is BaseStream sb) && EqualsInternal(sb);

		public override bool Equals(object? obj) => (obj is BaseStream sb) && EqualsInternal(sb);

		public static bool operator ==(BaseStream lhs, BaseStream rhs)
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

		public static bool operator !=(BaseStream lhs, BaseStream rhs)
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

		private bool EqualsInternal(BaseStream other) => Link.Equals(other.Link);

		public virtual int CompareTo(IStream? other)
		{
			if (other is null)
			{
				return -1;
			}

			return Comparer(this, other);
		}

		public static int Comparer(IStream x, IStream y)
		{
			ArgumentNullException.ThrowIfNull(x);
			ArgumentNullException.ThrowIfNull(y);

			int compareStatus = x.Status.CompareTo(y.Status);

			if (compareStatus != 0)
			{
				return compareStatus;
			}

			int compareNames = String.Compare(x.DisplayName, y.DisplayName, StringComparison.OrdinalIgnoreCase);

			if (compareNames != 0)
			{
				return compareNames;
			}

			int compareServiceNames = String.Compare(x.ServiceName, y.ServiceName, StringComparison.OrdinalIgnoreCase);

			return compareServiceNames;
		}

		public static bool operator >(BaseStream lhs, BaseStream rhs)
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

		public static bool operator >=(BaseStream lhs, BaseStream rhs)
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

		public static bool operator <(BaseStream lhs, BaseStream rhs)
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

		public static bool operator <=(BaseStream lhs, BaseStream rhs)
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
