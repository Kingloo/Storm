using System;
using System.IO;
using System.Reflection;
using System.Text;
using StormLib.Common;
using StormLib.Interfaces;

namespace StormLib.Streams
{
    public class UnsupportedStream : BindableBase, IStream
    {
        public Uri Link { get; }

        public Status Status
        {
            get => Status.Unsupported;
            set => throw new NotImplementedException("setting the Status of an UnsupportedStream is unsupported");
        }

        private string _name = "unset Name";
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value, nameof(Name));
        }

        public string DisplayName
        {
            get => Name;
            set => throw new NotImplementedException("setting DisplayName of an UnsupportedStream is unsupported");
        }
        
        public int ViewersCount
        {
            get => -1;
            set => throw new NotImplementedException("setting ViewersCount of an UnsupportedStream is unsupported");
        }

        private Uri? _icon = null;
        public Uri Icon
        {
            get
            {
                if (_icon is null)
                {
                    string libraryDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string iconsDirectory = "Icons";
                    string filename = "Unsupported.ico";

                    string fullPath = Path.Combine(libraryDirectory, iconsDirectory, filename);

                    _icon = new Uri(fullPath);
                }

                return _icon;
            }
        }

        public bool HasStreamlinkSupport => false;

        public UnsupportedStream(Uri uri)
        {
            Link = uri ?? throw new ArgumentNullException(nameof(uri));

            _name = uri.AbsoluteUri;
        }

        public int CompareTo(IStream other) => Name.CompareTo(other.Name);

        public bool Equals(IStream other) => (other is UnsupportedStream us) ? EqualsInternal(us) : false;

        public override bool Equals(object obj) => (obj is UnsupportedStream us) ? EqualsInternal(us) : false;

        public override int GetHashCode() => Link.GetHashCode();

        public static bool operator ==(UnsupportedStream lhs, UnsupportedStream rhs) => lhs.EqualsInternal(rhs);
        
        public static bool operator !=(UnsupportedStream lhs, UnsupportedStream rhs) => !lhs.EqualsInternal(rhs);

        private bool EqualsInternal(UnsupportedStream other) => Link.Equals(other.Link);

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(base.ToString());
            sb.AppendLine($"name: {Name}");
            sb.AppendLine($"link: {Link.AbsoluteUri}");

            return sb.ToString();
        }
    }
}
