using System;

namespace Storm.Wpf.StreamServices
{
    public interface IStream
    {
        Uri AccountLink { get; }
        Uri AccountIcon { get; }
        string AccountName { get; }
        string DisplayName { get; set; }
        bool IsLive { get; set; }
    }
}
