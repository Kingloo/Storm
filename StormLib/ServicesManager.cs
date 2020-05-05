using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using StormLib.Common;
using StormLib.Helpers;
using StormLib.Interfaces;
using StormLib.Services;

namespace StormLib
{
    public class ServicesManager : IServicesManager
    {
        public IDownload Downloader { get; private set; }

        private readonly Collection<IService> _services = new Collection<IService>();
        public IReadOnlyCollection<IService> Services => _services;

        public ServicesManager()
        {
            Downloader = new Download();
        }

        public ServicesManager(IDownload download)
        {
            Downloader = download;
        }

        public bool SetDownloader(IDownload download)
        {
            if (!Downloader.IsActive)
            {
                Downloader = download;

                return true;
            }
            else
            {
                return false;
            }
        }

        public void AddDefaultServices()
        {
            _services.Clear();

            _services.Add(new ChaturbateService(Downloader));
            _services.Add(new MixerService(Downloader));
            _services.Add(new MixlrService(Downloader));
            _services.Add(new TwitchService(Downloader));
        }

        public void AddService(IService service)
        {
            if (!_services.Any(s => s.HandlesStreamType == service.HandlesStreamType))
            {
                _services.Add(service);
            }
        }

        public void RemoveService(IService service)
        {
            IEnumerable<IService> servicesRegisteredForType = _services.Where(s => s.HandlesStreamType == service.HandlesStreamType);

            foreach (IService each in servicesRegisteredForType)
            {
                _services.Remove(each);
            }
        }

        public Task UpdateAsync(IEnumerable<IStream> streams)
            => UpdateAsync(streams, true);

        public async Task UpdateAsync(IEnumerable<IStream> streams, bool preserveSynchronizationContext)
        {
            List<Task<Result>> tasks = new List<Task<Result>>();

            foreach (IService service in _services)
            {
                IEnumerable<IStream> streamsForService = streams
                    .Where(stream => stream.GetType() == service.HandlesStreamType)
                    .ToList();

                Task<Result> task = Task.Run(() => service.UpdateAsync(streamsForService, preserveSynchronizationContext));

                tasks.Add(task);
            }

            await Task.WhenAll(tasks).ConfigureAwait(preserveSynchronizationContext);
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Downloader.Dispose();

                    foreach (IService each in _services)
                    {
                        each.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
