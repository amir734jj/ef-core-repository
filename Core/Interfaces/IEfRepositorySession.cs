namespace EfCoreRepository.Interfaces
{
    public interface IEfRepositorySession
    {
        IBasicCrud<TSource> For<TSource>() where TSource : class, IUntypedEntity;
    }
}