using System.Reflection;

namespace Core.Interfaces
{
    public interface IEfRepositoryFactory
    {
        IEfRepositoryFactory Profiles(params Assembly[] assemblies);
    }
}