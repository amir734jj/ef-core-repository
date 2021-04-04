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
    }
}