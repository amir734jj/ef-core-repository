using System.Linq;

namespace EfCoreRepository.Interfaces
{
    public interface IEntityProfile<TSource> where TSource : class, IUntypedEntity
    {
        /// <summary>
        /// Updated entity given dto
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="dto"></param>
        void Update(TSource entity, TSource dto);

        /// <summary>
        /// Intercept the IQueryable to include
        /// </summary>
        /// <returns></returns>
        IQueryable<TSource> Include<TQueryable>(TQueryable queryable) where TQueryable : IQueryable<TSource>;
    }
}