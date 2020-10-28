namespace EfCoreRepository.Interfaces
{
    public interface IEfRepository
    {
        /// <summary>
        /// Get basic CRUD
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TId"></typeparam>
        /// <returns></returns>
        IBasicCrudWrapper<TSource, TId> For<TSource, TId>() where TSource : class, IEntity<TId>;

        /// <summary>
        /// Session mode will delay the save changes until 
        /// </summary>
        /// <returns></returns>
        IEfRepositorySession Session();
    }
}