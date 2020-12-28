using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EfCoreRepository.Interfaces
{
    public interface IBasicCrud<TSource> where TSource : class, IUntypedEntity
    {
        Task<IEnumerable<TSource>> GetAll();

        Task<TSource> Get<TId>(TId id) where TId : struct;
        
        Task<IEnumerable<TSource>> GetWhere(Expression<Func<TSource, bool>> expression);

        Task<TSource> Save(TSource instance);

        Task<TSource> Delete<TId>(TId id) where TId : struct;

        Task<TSource> Update<TId>(TId id, TSource dto)where TId : struct;
    }
}