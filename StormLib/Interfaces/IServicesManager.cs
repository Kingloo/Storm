using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StormLib.Interfaces
{
    public interface IServicesManager : IDisposable
    {
        IDownload Downloader { get; }
        IReadOnlyCollection<IService> Services { get; }

        bool SetDownloader(IDownload download);

        void AddDefaultServices();
        void AddService(IService service);
        void RemoveService(IService service);

        Task UpdateAsync(IEnumerable<IStream> streams);
        Task UpdateAsync(IEnumerable<IStream> streams, bool preserveSynchronizationContext);
    }
}
