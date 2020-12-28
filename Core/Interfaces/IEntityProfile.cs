using System.Linq;

namespace EfCoreRepository.Interfaces
{
    public interface IEntityProfile<TSource, TId>
        where TSource : class, IEntity<TId>
    {
        /// <summary>
        /// Updated entity given dto
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="dto"></param>
        void Update(TSource entity, TSource dto)
        {
            // No change
        }

        /// <summary>
        /// Intercept the IQueryable to include
        /// </summary>
        /// <returns></returns>
        IQueryable<TSource> Include<TQueryable>(TQueryable queryable) where TQueryable : IQueryable<TSource>
        {
            return queryable;
        }
    }
}