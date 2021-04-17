using System.Reflection;

namespace EfCoreRepository.Interfaces
{
    public interface IEfRepositoryFactory
    {
        IEfRepositoryFactory Profile(params Assembly[] assemblies);

        IEfRepositoryFactory Profile<T>(params T[] profiles) where T : class, IEntityProfile;

        IEfRepositoryFactory Profile<T>() where T : class, IEntityProfile;
    }
}