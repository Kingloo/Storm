using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using StormLib.Common;
using StormLib.Interfaces;

namespace StormLib.Streams
{
    public class TwitchStream : BindableBase, IStream
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

        private string _game = string.Empty;
        public string Game
        {
            get => _game;
            set => SetProperty(ref _game, value, nameof(Game));
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
                    string filename = "Twitch.ico";

                    string fullPath = Path.Combine(libraryDirectory, iconsDirectory, filename);

                    _icon = new Uri(fullPath);
                }

                return _icon;
            }
        }

        public string Message => GetMessage();

        public TwitchStream(Uri uri)
        {
            Link = uri;
            
            _name = Link.Segments.Last(s => s != "/");
        }

        private string GetMessage()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(DisplayName);
            
            if (Status == Status.Public)
            {
                sb.Append(" is LIVE");

                if (!String.IsNullOrWhiteSpace(Game))
                {
                    sb.Append(" and playing ");
                    sb.Append(Game);
                }
            }

            return sb.ToString();
        }

        public bool Equals(IStream other) => Link.Equals(other.Link);

        public int CompareTo(IStream other) => Name.CompareTo(other.Name);

        public override bool Equals(object obj) => (obj is TwitchStream ts) ? Equals(ts) : false;

        public override int GetHashCode() => Link.GetHashCode();

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(base.ToString());
            sb.AppendLine($"display name: {DisplayName}");
            sb.AppendLine($"status: {Status.ToString()}");
            sb.AppendLine($"viewers: {ViewersCount}");

            return sb.ToString();
        }
    }
}
