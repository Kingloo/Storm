using System;
using System.Collections.ObjectModel;
using System.Linq;
using Storm.Wpf.Streams;

namespace Storm.Wpf.StreamServices
{
    public static class ServicesManager
    {
        private static readonly Collection<StreamServiceBase> services = new Collection<StreamServiceBase>
        {
            new TwitchService(),
            new YouTubeService(),
            new ChaturbateService(),
            new MixerService(),
            new MixlrService(),
            new SmashcastService()
        };

        public static StreamServiceBase GetService(Type streamType)
        {
            try
            {
                return services.SingleOrDefault(service => service.HandlesStreamType == streamType);
            }
            catch (ArgumentNullException ex)
            {
                throw new ServicesException(ex.Message, streamType);
            }
        }

        public static Action StartWatching(StreamBase stream)
        {
            if (stream is null) { throw new ArgumentNullException(nameof(stream)); }

            var service = GetService(stream.GetType());

            return service.GetWatchingInstructions(stream);
        }

        public static Action StartRecording(StreamBase stream)
        {
            if (stream is null) { throw new ArgumentNullException(nameof(stream)); }

            var service = GetService(stream.GetType());

            return service.GetRecordingInstructions(stream);
        }
    }
}
