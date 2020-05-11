using System;
using System.IO;
using System.Linq;
using System.Reflection;
using StormLib.Common;
using StormLib.Interfaces;

namespace StormLib.Streams
{
    public class ChaturbateStream : BindableBase, IStream
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

        private Uri? _icon = null;
        public Uri Icon
        {
            get
            {
                if (_icon is null)
                {
                    string libraryDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string iconsDirectory = "Icons";
                    string filename = "Chaturbate.ico";

                    string fullPath = Path.Combine(libraryDirectory, iconsDirectory, filename);

                    _icon = new Uri(fullPath);
                }

                return _icon;
            }
        }

        public bool HasStreamlinkSupport => true;

        public string ServiceName => "Chaturbate";

        public ChaturbateStream(Uri uri)
        {
            Link = uri;

            Name = Link.Segments.Last(s => s != "/");
        }

        public int CompareTo(IStream other) => String.Compare(Name, other.Name, StringComparison.Ordinal);

        public bool Equals(IStream other) => Link.Equals(other.Link);

        public override bool Equals(object obj) => (obj is ChaturbateStream cs) ? Equals(cs) : false;

        public override int GetHashCode() => Link.GetHashCode();
    }
}
