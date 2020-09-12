using System;
using System.IO;
using System.Linq;
using System.Reflection;
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

        private int _viewersCount = -1;
        public int ViewersCount
        {
            get => _viewersCount;
            set => SetProperty(ref _viewersCount, value, nameof(ViewersCount));
        }

        protected static string IconDirectory => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Icons");

        public abstract Uri Icon { get; }
        public abstract bool HasStreamlinkSupport { get; }
        public abstract string ServiceName { get; }

        protected StreamBase(Uri uri)
        {
            Link = uri ?? throw new ArgumentNullException(nameof(uri));

            _name = GetName(Link);
        }

        protected virtual string GetName(Uri uri)
        {
            return uri.Segments.FirstOrDefault(s => s != "/")?.TrimEnd(Char.Parse("/")) ?? uri.AbsoluteUri;
        }

        public bool Equals(IStream other) => (other is StreamBase sb) && EqualsInternal(sb);

        public override bool Equals(object obj) => (obj is StreamBase sb) && EqualsInternal(sb);

        public static bool operator ==(StreamBase lhs, StreamBase rhs) => lhs.Equals(rhs);

        public static bool operator !=(StreamBase lhs, StreamBase rhs) => !lhs.Equals(rhs);

        private bool EqualsInternal(StreamBase other) => Link.Equals(other.Link);

        public virtual int CompareTo(IStream other) => String.Compare(Name, other.Name, StringComparison.Ordinal);

        public static bool operator >(StreamBase lhs, StreamBase rhs) => lhs.CompareTo(rhs) > 0;

        public static bool operator >=(StreamBase lhs, StreamBase rhs) => lhs.CompareTo(rhs) > 0;

        public static bool operator <(StreamBase lhs, StreamBase rhs) => lhs.CompareTo(rhs) < 0;

        public static bool operator <=(StreamBase lhs, StreamBase rhs) => lhs.CompareTo(rhs) <= 0;

        public override int GetHashCode() => Link.GetHashCode();

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(base.ToString());
            sb.AppendLine($"link: {Link.AbsoluteUri}");
            sb.AppendLine($"status: {Status}");
            sb.AppendLine($"name: {Name}");
            sb.AppendLine($"display name: {DisplayName}");
            sb.AppendLine($"viewers: {ViewersCount}");
            sb.AppendLine($"icon path: {Icon.AbsoluteUri}");
            sb.AppendLine($"has streamlink support: {HasStreamlinkSupport}");
            sb.AppendLine($"service name: {ServiceName}");

            return sb.ToString();
        }
    }
}
