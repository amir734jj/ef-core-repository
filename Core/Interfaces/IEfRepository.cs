namespace EfCoreRepository.Interfaces
{
    public interface IEfRepository
    {
        /// <summary>
        /// Get basic CRUD
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        IBasicCrudWrapper<TSource> For<TSource>() where TSource : class, IUntypedEntity;

        /// <summary>
        /// Session mode will delay the save changes until 
        /// </summary>
        /// <returns></returns>
        IEfRepositorySession Session();
    }
}