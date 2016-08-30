using System.Collections.Generic;
using System.Threading.Tasks;
using Storm.Model;

namespace Storm.DataAccess
{
    public interface IRepository
    {
        string FilePath { get; }

        void SetFilePath(string newPath);

        Task<IEnumerable<StreamBase>> LoadAsync();
        Task SaveAsync(IEnumerable<StreamBase> streams);
    }
}
