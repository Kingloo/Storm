using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
            int count = services.Count(service => service.HandlesStreamType == streamType);

            if (count == 1)
            {
                return services.Single(service => service.HandlesStreamType == streamType);
            }
            else
            {
                string message = $"There were {count} registered services for {streamType.FullName}. There must only be one.";

                throw new ServicesException(count, message, streamType);
            }
        }

        public static void StartWatching(StreamBase stream)
        {
            var service = GetService(stream.GetType());

            ProcessStartInfo watchingInfo = service.GetWatchingInstructions(stream);

            Start(watchingInfo);
        }

        public static void StartRecording(StreamBase stream)
        {
            var service = GetService(stream.GetType());

            ProcessStartInfo recordingInfo = service.GetRecordingInstructions(stream);

            Start(recordingInfo);
        }

        private static void Start(ProcessStartInfo info)
        {
            Process process = new Process
            {
                StartInfo = info
            };

            process.Start();
        }
    }
}
