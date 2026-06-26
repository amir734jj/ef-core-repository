using System.Reflection;

namespace EfCoreRepository.Interfaces
{
    public interface IEfRepositoryFactory
    {
        IEfRepositoryFactory Profile(params Assembly[] assemblies);

        /// <summary>
        /// Makes profiles optional: every entity type exposed by the DbContext (a class with a
        /// discoverable key) that has no explicit profile gets a default one that auto-maps all
        /// properties (<c>MapAll</c>) and adds no eager includes.
        /// </summary>
        IEfRepositoryFactory DefaultProfiles();

        IEfRepositoryFactory Profile<TProfile, TEntity>(TProfile profile)
            where TProfile : EntityProfile<TEntity>
            where TEntity : class;

        IEfRepositoryFactory Profile<TProfile, TEntity>()
            where TProfile : EntityProfile<TEntity>
            where TEntity : class;
    }
}