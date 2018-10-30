using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Storm.Wpf.Streams;

namespace Storm.Wpf.StreamServices
{
    public static class ServicesManager
    {
        public static async Task UpdateAsync(IEnumerable<IStream> streams)
        {
            await TwitchService.UpdateAsync(
                streams
                .Where(stream => stream.GetType() == typeof(TwitchStream))
                .Cast<TwitchStream>()
                );

            await ChaturbateService.UpdateAsync(
                streams
                .Where(stream => stream.GetType() == typeof(ChaturbateStream))
                .Cast<ChaturbateStream>()
                );
        }
    }
}
