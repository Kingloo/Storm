using System;
using System.Collections.ObjectModel;
using System.Linq;
using Storm.Wpf.Common;
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
            int count = services.Count(service => service.HandlesStreamType == streamType);

            if (count == 1)
            {
                try
                {
                    return services.Single(service => service.HandlesStreamType == streamType);
                }
                catch (InvalidOperationException ex)
                {
                    Log.LogException(ex, "StreamServices->GetService->.Single");

                    return null;
                }
            }
            else
            {
                string message = $"There were {count} registered services for {streamType.FullName}. There must only be one.";

                throw new ServicesException(count, message, streamType);
            }
        }

        public static Action StartWatching(StreamBase stream)
        {
            var service = GetService(stream.GetType());

            return service.GetWatchingInstructions(stream);
        }

        public static Action StartRecording(StreamBase stream)
        {
            var service = GetService(stream.GetType());

            return service.GetRecordingInstructions(stream);
        }
    }
}
