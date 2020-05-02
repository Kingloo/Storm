using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace StormLib.Interfaces
{
    public interface IDownload : IDisposable
    {
        bool IsActive { get; }

        Task<(HttpStatusCode, string)> StringAsync(Uri uri);
        Task<(HttpStatusCode, string)> StringAsync(HttpRequestMessage request);
        Task<(HttpStatusCode, byte[])> DataAsync(Uri uri);
        Task<(HttpStatusCode, byte[])> DataAsync(HttpRequestMessage request);
    }
}
