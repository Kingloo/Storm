using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Storm.Wpf.Streams;

namespace Storm.Wpf.StreamServices
{
    public static class ServicesManager
    {
        public static Task UpdateAsync(IEnumerable<IStream> streams)
        {
            var updateTasks = new List<Task>
            {
                TwitchService.UpdateAsync(streams.OfType<TwitchStream>()),
                ChaturbateService.UpdateAsync(streams.OfType<ChaturbateStream>())
            };

            return Task.WhenAll(updateTasks);
        }
    }
}
