using System.Collections.Generic;

namespace Storm.Wpf.StreamServices
{
    public interface IServiceRequest
    {
        IEnumerable<string> UserNames { get; }
    }
}
