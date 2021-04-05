using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace EfCoreRepository.Interfaces
{
    public interface IEfRepositoryFactory
    {
        IEfRepositoryFactory Profiles(params Assembly[] assemblies);

        IEfRepositoryFactory Profiles<T>(params T[] profiles) where T : class, IEntityProfile;
    }
}