using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EfCoreRepository.Interfaces
{
    public interface IBasicCrud<TSource> : IDisposable, IAsyncDisposable where TSource : class
    {
        #region Utilities

        /// <summary>
        /// For complex and multi-action where we want to defer the save until the dispose takes place
        /// </summary>
        /// <returns></returns>
        IBasicCrud<TSource> Delayed();
        
        /// <summary>
        /// Avoids eager loading altogether for a lightweight session
        /// </summary>
        /// <returns></returns>
        IBasicCrud<TSource> Light();

        #endregion
        
        #region Basics

        Task<TSource> Save(TSource instance);
        
        Task<IEnumerable<TSource>> GetAll();

        #endregion
        
        #region IdAware
        
        Task<TSource> Get<TId>(TId id) where TId : struct;
        
        Task<TSource> Delete<TId>(TId id) where TId : struct;

        Task<TSource> Update<TId>(TId id, TSource dto) where TId : struct;

        Task<TSource> Update<TId>(TId id, Action<TSource> updater) where TId : struct;

        #endregion

        #region IdUnAware

        Task<bool> Any(Expression<Func<TSource, bool>> expression);
        
        Task<bool> All(Expression<Func<TSource, bool>> expression);
        
        Task<TSource> Get(Expression<Func<TSource, bool>> expression);

        Task<TSource> Update(Expression<Func<TSource, bool>> expression, TSource dto);

        Task<TSource> Update(Expression<Func<TSource, bool>> expression, Action<TSource> updater);

        Task<TSource> Delete(Expression<Func<TSource, bool>> expression);
        
        #endregion

        #region Misc

        Task<IEnumerable<TSource>> GetAll(Expression<Func<TSource, bool>> filter);

        Task<IEnumerable<TSource>> GetAll<TId>(params TId[] ids) where TId : struct;

        Task<IEnumerable<TSource>> Save(params TSource[] instances);

        Task<int> Count(Expression<Func<TSource, bool>> expression);
        
        Task<int> Count();

        #endregion
    }
}