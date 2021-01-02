using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using EfCoreRepository.Interfaces;
using EfCoreRepository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EfCoreRepository
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
                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityProfile<>))
                ))
                .Where(x => x.GenericType != null)
                .ToList();
            
            // DbContext is registered by default as scoped
            // https://docs.microsoft.com/en-us/ef/core/miscellaneous/configuring-dbcontext#implicitly-sharing-dbcontext-instances-across-multiple-threads-via-dependency-injection
            _serviceCollection.Add(ServiceDescriptor.Scoped(typeof(IEntityProfileAuxiliary), typeof(EntityProfileAuxiliary)));

            _serviceCollection.Add(ServiceDescriptor.Scoped<IEfRepository>(serviceProvider => new EfRepository(profiles.Select(tuple =>
            {
                var (sourceType, genericType) = tuple;

                if (genericType == null)
                {
                    throw new Exception("Profiles generic type is null");
                }
                    
                return new EntityProfileAttributed
                {
                    SourceType = genericType.GetGenericArguments().First(),
                    Profile = ActivatorUtilities.CreateInstance(serviceProvider, sourceType)
                };
            }), serviceProvider.GetService<TDbContext>(), false)));
        }
    }
}