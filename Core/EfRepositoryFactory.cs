using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using EfCoreRepository.Interfaces;
using EfCoreRepository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EfCoreRepository
{
    internal class EfRepositoryFactory<TDbContext> : IEfRepositoryFactory where TDbContext: DbContext
    {
        private readonly IServiceCollection _serviceCollection;

        private List<(Type SourceType, Type GenericType)> _profiles;

        public EfRepositoryFactory(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
            _profiles = new List<(Type SourceType, Type GenericType)>();
        }
        
        public IEfRepositoryFactory Profiles(params Assembly[] assemblies)
        {
            _profiles = _profiles.Concat(assemblies
                .SelectMany(assembly => assembly.GetExportedTypes())
                .Select(type => (
                    SourceType: type,
                    GenericType: type.GetInterfaces().FirstOrDefault(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityProfile<>))
                ))
                .Where(x => x.GenericType != null)).ToList();

            return this;
        }

        public IEfRepositoryFactory Profiles<T>(params T[] profiles) where T : class, IEntityProfile
        {
            _profiles = _profiles.Concat(profiles.Select(x => (
                SourceType: x.GetType(),
                GenericType: x.GetType().GetInterfaces().FirstOrDefault(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityProfile<>))
            ))).ToList();

            return this;
        }

        public void Build()
        {
            AddEfRepository<TDbContext>(_profiles);
        }

        private void AddEfRepository<TDbContext>(IReadOnlyCollection<(Type SourceType, Type GenericType)> profiles) where TDbContext: DbContext
        {
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
            }), serviceProvider.GetService<TDbContext>())));
        }
    }
}