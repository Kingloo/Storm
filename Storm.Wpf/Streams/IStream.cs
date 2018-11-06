using System;

namespace Storm.Wpf.Streams
{
    public interface IStream
    {
        Uri AccountLink { get; }
        Uri Icon { get; }
        string AccountName { get; }
        string DisplayName { get; set; }
        bool IsLive { get; set; }
        bool AutoRecord { get; set; }
    }
}
