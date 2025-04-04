using System.Collections.Generic;
using System.Threading.Tasks;

namespace EfCoreRepository.Interfaces;

public interface IBasicCrudUnSafe<TSource> where TSource : class
{
    Task<int> Count();
    
    Task<IEnumerable<TSource>> GetAll();

    Task<bool> Any();
}