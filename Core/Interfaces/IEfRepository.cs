namespace EfCoreRepository.Interfaces
{
    public interface IEfRepository
    {
        IBasicCrud<TSource, TId> For<TSource, TId>() where TSource : class, IEntity<TId>;
    }
}