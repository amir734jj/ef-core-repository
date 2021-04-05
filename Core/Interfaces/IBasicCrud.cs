using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EfCoreRepository.Interfaces
{
    public interface IBasicCrud<TSource> where TSource : class
    {
        #region Basics

        Task<TSource> Save(TSource instance);
        
        Task<IEnumerable<TSource>> GetAll();

        #endregion
        
        #region IdAware
        
        Task<TSource> Get<TId>(TId id) where TId : struct;
        
        Task<TSource> Delete<TId>(TId id) where TId : struct;

        Task<TSource> Update<TId>(TId id, TSource dto) where TId : struct;

        #endregion

        #region IdUnAware

        Task<TSource> Get(Expression<Func<TSource, bool>> expression);

        Task<TSource> Update(Expression<Func<TSource, bool>> expression, TSource dto);
        
        Task<TSource> Delete(Expression<Func<TSource, bool>> expression);
        
        #endregion

        #region Misc

        Task<IEnumerable<TSource>> GetAll(Expression<Func<TSource, bool>> filter);

        Task<IEnumerable<TSource>> GetAll<TId>(params TId[] ids) where TId : struct;

        Task<IEnumerable<TSource>> Save(params TSource[] instances);
        
        #endregion
    }
}