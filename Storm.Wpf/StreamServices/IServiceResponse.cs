using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Storm.Wpf.StreamServices
{
    public interface IServiceResponse
    {
        Dictionary<string, string> DisplayNames { get; }
        Collection<string> UserNamesThatAreLive { get; }
    }
}
