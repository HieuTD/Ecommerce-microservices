using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Core.Repositories
{
    public interface ITypeRepository
    {
        Task<IEnumerable<Type>> GetAllTypes();
    }
}
