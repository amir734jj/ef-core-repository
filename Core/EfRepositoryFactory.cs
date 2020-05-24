using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Core.Interfaces;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Core
{
    internal class EfRepositoryFactory : IEfRepositoryFactory
    {
        private readonly IServiceCollection _serviceCollection;

        private ImmutableList<Assembly> _sourceAssemblies;

        public EfRepositoryFactory(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
            _sourceAssemblies = ImmutableList<Assembly>.Empty;
        }
        
        public IEfRepositoryFactory Profiles(params Assembly[] assemblies)
        {
            _sourceAssemblies = _sourceAssemblies.AddRange(assemblies);

            return this;
        }

        public void Build<TDbContext>() where TDbContext: DbContext
        {
            var profiles = _sourceAssemblies
                .SelectMany(assembly => assembly.GetExportedTypes())
                .Select(type => (
                    SourceType: type,
                    GenericType: type.GetInterfaces().FirstOrDefault(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityProfile<,>))
                ))
                .Where(x => x.GenericType != null)
                .ToList();
            
            _serviceCollection.Add(ServiceDescriptor.Singleton(typeof(IEntityProfileAuxiliary<,>), typeof(EntityProfileAuxiliary<,>)));

            _serviceCollection.Add(ServiceDescriptor.Singleton<IEfRepository>(serviceProvider =>
            {
                return new EfRepository(profiles.Select(tuple => new EntityProfileAttributed
                {
                    SourceType = tuple.GenericType.GetGenericArguments().First(),
                    IdType = tuple.GenericType.GetGenericArguments().Last(),
                    Profile = ActivatorUtilities.CreateInstance(serviceProvider, tuple.SourceType)
                }), serviceProvider.GetService<TDbContext>());
            }));
        }
    }
}