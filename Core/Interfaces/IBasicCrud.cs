using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EfCoreRepository.Interfaces
{
    public interface IBasicCrud<TSource> where TSource : class, IUntypedEntity
    {
        #region IdAware

        Task<IEnumerable<TSource>> GetAll();

        Task<TSource> Get<TId>(TId id) where TId : struct;

        Task<TSource> Save(TSource instance);

        Task<TSource> Delete<TId>(TId id) where TId : struct;

        Task<TSource> Update<TId>(TId id, TSource dto) where TId : struct;

        #endregion

        #region IdUnAware

        Task<TSource> Get(Expression<Func<TSource, bool>> expression);

        Task<TSource> Update(Expression<Func<TSource, bool>> expression, TSource dto);
        
        Task<TSource> Delete(Expression<Func<TSource, bool>> expression);
        
        #endregion
    }
}