using System;
using System.Collections.Generic;
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
        private readonly ServiceLifetime _serviceLifetime;
        private readonly List<(Type SourceType, Type GenericType)> _profiles;

        public EfRepositoryFactory(IServiceCollection serviceCollection, ServiceLifetime serviceLifetime)
        {
            _serviceCollection = serviceCollection;
            _serviceLifetime = serviceLifetime;
            _profiles = new List<(Type SourceType, Type GenericType)>();
        }
        
        public IEfRepositoryFactory Profile(params Assembly[] assemblies)
        {
            _profiles.AddRange(assemblies
                .SelectMany(assembly => assembly.GetExportedTypes())
                .Select(type => (
                    SourceType: type,
                    GenericType: GetProfileGenericType(type)
                ))
                .Where(x => x.GenericType != null));

            return this;
        }

        public IEfRepositoryFactory Profile<TProfile, TEntity>(TProfile profile)
            where TProfile : EntityProfile<TEntity>
            where TEntity : class
        {
            var t = profile.GetType();

            _profiles.Add((t, GetProfileGenericType(t)));

            return this;
        }

        public IEfRepositoryFactory Profile<TProfile, TEntity>()
            where TProfile : EntityProfile<TEntity>
            where TEntity : class
        {
            var t = typeof(TProfile);
            _profiles.Add((t, GetProfileGenericType(t)));

            return this;
        }

        public void Build()
        {
            AddEfRepository(_profiles, _serviceLifetime);
        }

        private void AddEfRepository(IReadOnlyCollection<(Type SourceType, Type GenericType)> profiles, ServiceLifetime serviceLifetime)
        {
            // DbContext is registered by default as scoped
            // https://docs.microsoft.com/en-us/ef/core/miscellaneous/configuring-dbcontext#implicitly-sharing-dbcontext-instances-across-multiple-threads-via-dependency-injection
            _serviceCollection.Add(ServiceDescriptor.Describe(typeof(IEfRepository), serviceProvider => new EfRepository(profiles.Select(tuple =>
            {
                var (sourceType, genericType) = tuple;

                if (genericType == null)
                {
                    throw new Exception($"Profiles generic type is null for {sourceType.Name}");
                }

                return new EntityProfileAttributed
                {
                    SourceType = genericType.GetGenericArguments().First(),
                    Profile = ActivatorUtilities.CreateInstance(serviceProvider, sourceType)
                };
            }).ToList(), serviceProvider.GetService<TDbContext>()), serviceLifetime));
        }

        private static Type GetProfileGenericType(Type t)
        {
            if (t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(EntityProfile<>))
            {
                return t.BaseType;
            }

            return null;
        }
    }
}