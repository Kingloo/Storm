//using System;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using StormLib.Common;
//using StormLib.Interfaces;

//namespace StormLib.Streams
//{
//    public class MixerStream : BindableBase, IStream
//    {
//        public Uri Link { get; }

//        private Status _status = Status.None;
//        public Status Status
//        {
//            get => _status;
//            set => SetProperty(ref _status, value, nameof(Status));
//        }

//        private string _name = "unset Name";
//        public string Name
//        {
//            get => _name;
//            set => SetProperty(ref _name, value, nameof(Name));
//        }

//        private string _displayName = string.Empty;
//        public string DisplayName
//        {
//            get => String.IsNullOrWhiteSpace(_displayName) ? _name : _displayName;
//            set => SetProperty(ref _displayName, value, nameof(DisplayName));
//        }

//        private int _viewersCount = -1;
//        public int ViewersCount
//        {
//            get => _viewersCount;
//            set => SetProperty(ref _viewersCount, value, nameof(ViewersCount));
//        }

//        private string _game = string.Empty;
//        public string Game
//        {
//            get => _game;
//            set => SetProperty(ref _game, value, nameof(Game));
//        }

//        private Uri? _icon = null;
//        public Uri Icon
//        {
//            get
//            {
//                if (_icon is null)
//                {
//                    string libraryDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//                    string iconsDirectory = "Icons";
//                    string filename = "Mixer.ico";

//                    string fullPath = Path.Combine(libraryDirectory, iconsDirectory, filename);

//                    _icon = new Uri(fullPath);
//                }

//                return _icon;
//            }
//        }

//        public bool HasStreamlinkSupport => true;

//        public string ServiceName => "Mixer";

//        public MixerStream(Uri uri)
//        {
//            Link = uri ?? throw new ArgumentNullException(nameof(uri));

//            _name = Link.Segments.FirstOrDefault(s => s != "/")?.TrimEnd(Char.Parse("/")) ?? Link.AbsoluteUri;
//        }

//        public bool Equals(IStream other) => (other is MixerStream ms) ? EqualsInternal(ms) : false;

//        public override bool Equals(object obj) => (obj is MixerStream ms) ? EqualsInternal(ms) : false;

//        private bool EqualsInternal(MixerStream other) => Link.Equals(other.Link);

//        public static bool operator ==(MixerStream lhs, MixerStream rhs) => lhs.Equals(rhs);

//        public static bool operator !=(MixerStream lhs, MixerStream rhs) => !lhs.Equals(rhs);

//        public int CompareTo(IStream other) => String.Compare(Name, other.Name, StringComparison.Ordinal);

//        public static bool operator >(MixerStream lhs, MixerStream rhs) => lhs.CompareTo(rhs) > 0;

//        public static bool operator >=(MixerStream lhs, MixerStream rhs) => lhs.CompareTo(rhs) > 0;

//        public static bool operator <(MixerStream lhs, MixerStream rhs) => lhs.CompareTo(rhs) < 0;

//        public static bool operator <=(MixerStream lhs, MixerStream rhs) => lhs.CompareTo(rhs) <= 0;

//        public override int GetHashCode() => Link.GetHashCode();

//        public override string ToString()
//        {
//            StringBuilder sb = new StringBuilder();

//            sb.AppendLine(base.ToString());
//            sb.AppendLine($"status: {Status}");
//            sb.AppendLine($"name: {Name}");
//            sb.AppendLine($"display name: {DisplayName}");
//            sb.AppendLine($"viewers: {ViewersCount}");

//            return sb.ToString();
//        }
//    }
//}


using System;

namespace StormLib.Streams
{
    public class MixerStream : StreamBase
    {
        public override bool HasStreamlinkSupport => true;

        public override string ServiceName => "Mixer";

        private string _game = string.Empty;
        public string Game
        {
            get => _game;
            set => SetProperty(ref _game, value, nameof(Game));
        }

        public MixerStream(Uri uri)
            : base(uri)
        { }
    }
}