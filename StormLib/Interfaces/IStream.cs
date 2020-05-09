using System;

namespace StormLib.Interfaces
{
    public interface IStream : IEquatable<IStream>, IComparable<IStream>
    {
        Uri Link { get; }
        Status Status { get; set; }
        string Name { get; }
        string DisplayName { get; set; }
        int ViewersCount { get; set; }
        Uri Icon { get; }
        bool HasStreamlinkSupport { get; }
        string ServiceName { get; }
    }
}
