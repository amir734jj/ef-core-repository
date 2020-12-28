using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EfCoreRepository.Interfaces
{
    public interface IBasicCrud<TSource, in TId> where TSource : class, IEntity<TId>
    {
        Task<IEnumerable<TSource>> GetAll();

        Task<TSource> Get(TId id);
        
        Task<IEnumerable<TSource>> GetWhere(Expression<Func<TSource, bool>> expression);

        Task<TSource> Save(TSource instance);

        Task<TSource> Delete(TId id);

        Task<TSource> Update(TId id, TSource dto);
    }
}