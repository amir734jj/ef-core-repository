using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IBasicCrud<TSource, in TId> where TSource: class, IEntity<TId>
    {
        Task<IEnumerable<TSource>> GetAll();

        Task<TSource> Get(TId id);

        Task<TSource> Save(TSource instance);
        
        Task<TSource> Delete(TId id);

        Task<TSource> Update(TId id, TSource dto);
    }
}