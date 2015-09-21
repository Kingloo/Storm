using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storm.DataAccess
{
    public interface IRepository
    {
        string FilePath { get; set; }

        Task<IEnumerable<StreamBase>> LoadAsync();
        Task SaveAsync(IEnumerable<StreamBase> uris);
    }
}
