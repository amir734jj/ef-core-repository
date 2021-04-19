using System;
using System.Collections.Generic;
using System.Linq;
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
        public IBasicCrud<TSource> Delayed();
        
        /// <summary>
        /// Avoids eager loading altogether for a lightweight session
        /// </summary>
        /// <returns></returns>
        public IBasicCrud<TSource> Light();

        #endregion
        
        #region Basics

        public Task<TSource> Save(TSource instance);
        
        public Task<IEnumerable<TSource>> GetAll();

        #endregion
        
        #region IdAware
        
        public Task<TSource> Get<TId>(TId id) where TId : struct;
        
        public Task<TSource> Delete<TId>(TId id) where TId : struct;

        public Task<TSource> Update<TId>(TId id, TSource dto) where TId : struct;

        #endregion

        #region IdUnAware

        public Task<TSource> Get(Expression<Func<TSource, bool>> expression);

        public  Task<TSource> Update(Expression<Func<TSource, bool>> expression, TSource dto);
        
        public Task<TSource> Delete(Expression<Func<TSource, bool>> expression);
        
        #endregion

        #region Misc

        public Task<IEnumerable<TSource>> GetAll(Expression<Func<TSource, bool>> filter);

        public Task<IEnumerable<TSource>> GetAll<TId>(params TId[] ids) where TId : struct;

        public Task<IEnumerable<TSource>> Save(params TSource[] instances);

        public IQueryable<TSource> DbSet();

        public Task<int> Count(Expression<Func<TSource, bool>> expression);
        
        public Task<int> Count();

        #endregion
    }
}