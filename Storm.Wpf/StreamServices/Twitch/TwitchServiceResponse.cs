using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Storm.Wpf.StreamServices.Twitch
{
    public class TwitchServiceResponse : IServiceResponse
    {
        public Dictionary<string, string> DisplayNames { get; } = new Dictionary<string, string>();
        public Collection<string> UserNamesThatAreLive { get; } = new Collection<string>();

        public TwitchServiceResponse() { }
    }
}
