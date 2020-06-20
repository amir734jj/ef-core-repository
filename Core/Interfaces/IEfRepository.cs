namespace EfCoreRepository.Interfaces
{
    public interface IEfRepository
    {
        IBasicCrudType<TSource, TId> For<TSource, TId>() where TSource : class, IEntity<TId>;
    }
}