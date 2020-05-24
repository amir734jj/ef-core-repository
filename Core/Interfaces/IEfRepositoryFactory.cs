using System.Reflection;

namespace EfCoreRepository.Interfaces
{
    public interface IEfRepositoryFactory
    {
        IEfRepositoryFactory Profiles(params Assembly[] assemblies);
    }
}