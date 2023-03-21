using System.Collections.Generic;
using System.Threading.Tasks;

namespace EfCoreRepository.Interfaces
{
    public interface IBasicCrudUnsafe<TSource> where TSource : class
    {
        /// <summary>
        /// Unsafe
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TSource>> GetAll();
        
        /// <summary>
        /// Okay.
        /// </summary>
        /// <returns></returns>
        Task<int> Count();
    }
}