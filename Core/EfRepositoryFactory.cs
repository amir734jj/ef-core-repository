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

        private readonly List<(Type SourceType, Type GenericType)> _profiles;

        public EfRepositoryFactory(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
            _profiles = new List<(Type SourceType, Type GenericType)>();
        }
        
        public IEfRepositoryFactory Profile(params Assembly[] assemblies)
        {
            _profiles.AddRange(assemblies
                .SelectMany(assembly => assembly.GetExportedTypes())
                .Select(type => (
                    SourceType: type,
                    GenericType: GetProfileGenericType(type)
                )));

            return this;
        }

        public IEfRepositoryFactory Profile<T>(params T[] profiles) where T : class, IEntityProfile
        {
            _profiles.AddRange(profiles.Select(x => x.GetType()).Select(t => (
                SourceType: t,
                GenericType: GetProfileGenericType(t))));

            return this;
        }

        public IEfRepositoryFactory Profile<T>() where T : class, IEntityProfile
        {
            var t = typeof(T);
            _profiles.Add((t, GetProfileGenericType(t)));

            return this;
        }

        public void Build()
        {
            AddEfRepository(_profiles);
        }

        private void AddEfRepository(IReadOnlyCollection<(Type SourceType, Type GenericType)> profiles)
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
            }).ToList(), serviceProvider.GetService<TDbContext>())));
        }

        private static Type GetProfileGenericType(Type t)
        {
            return t.GetInterfaces().FirstOrDefault(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityProfile<>));
        }
    }
}