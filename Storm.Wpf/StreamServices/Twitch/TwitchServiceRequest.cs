using System;
using System.Collections.Generic;
using System.Linq;

namespace Storm.Wpf.StreamServices.Twitch
{
    public class TwitchServiceRequest : IServiceRequest
    {
        public IEnumerable<string> UserNames { get; } = Enumerable.Empty<string>();

        public TwitchServiceRequest(IEnumerable<string> userNames)
        {
            UserNames = userNames ?? throw new ArgumentNullException(nameof(userNames));
        }
    }
}
