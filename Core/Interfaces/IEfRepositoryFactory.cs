using System.Reflection;

namespace EfCoreRepository.Interfaces
{
    public interface IEfRepositoryFactory
    {
        public IEfRepositoryFactory Profile(params Assembly[] assemblies);

        public IEfRepositoryFactory Profile<TProfile, TEntity>(TProfile profile)
            where TProfile : EntityProfile<TEntity>
            where TEntity : class;

        public IEfRepositoryFactory Profile<TProfile, TEntity>()
            where TProfile : EntityProfile<TEntity>
            where TEntity : class;
    }
}