using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EfCoreRepository.Interfaces;
using EfCoreRepository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static EfCoreRepository.EntityUtility;

namespace EfCoreRepository
{
    internal class EfRepositoryFactory<TDbContext>(
        IServiceCollection serviceCollection,
        ServiceLifetime serviceLifetime)
        : IEfRepositoryFactory
        where TDbContext : DbContext
    {
        private readonly List<(Type ProfileType, Type EntityType)> _context = [];

        public IEfRepositoryFactory Profile(params Assembly[] assemblies)
        {
            _context.AddRange(assemblies
                .SelectMany(assembly => assembly.GetExportedTypes())
                .Where(x => typeof(IEntityProfile).IsAssignableFrom(x))
                .Select(type => (
                    ProfileType: type,
                    EntityType: GetProfileGenericType(type)
                ))
                .Where(x => x.EntityType != null));

            return this;
        }

        public IEfRepositoryFactory Profile<TProfile, TEntity>(TProfile profile)
            where TProfile : EntityProfile<TEntity>
            where TEntity : class
        {
            var t = profile.GetType();

            _context.Add((t, GetProfileGenericType(t)));

            return this;
        }

        public IEfRepositoryFactory Profile<TProfile, TEntity>()
            where TProfile : EntityProfile<TEntity>
            where TEntity : class
        {
            var t = typeof(TProfile);
            _context.Add((t, GetProfileGenericType(t)));

            return this;
        }

        public void Build()
        {
            AddEfRepository(_context, serviceLifetime);
        }

        private void AddEfRepository(IReadOnlyCollection<(Type ProfileType, Type EntityType)> context, ServiceLifetime serviceLifetime)
        {
            var entityTypes = context.Select(x => x.EntityType).ToList();
            var duplicateProfiles =  context.GroupBy(x => x.EntityType).Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .ToList();
            
            // Check for duplicate profiles
            if (duplicateProfiles.Any())
            {
                throw new Exception(
                    $"Duplicate profile has been defined for entities: {string.Join(", ", duplicateProfiles.Select(x => x.Name))}");
            }

            var missingKeys = context.Where(x =>
            {
                try
                {
                    FindIdProperty(x.EntityType);

                    return false;
                }
                catch (Exception)
                {
                    return true;
                }
            }).ToList();
            
            // Check for missing ID property in Entity
            if (duplicateProfiles.Any())
            {
                throw new Exception($"Missing KEY attribute on the class declaration for entities: {string.Join(", ", missingKeys.Select(x => x.EntityType.Name))}");
            }
            
            // DbContext is registered by default as scoped
            // https://docs.microsoft.com/en-us/ef/core/miscellaneous/configuring-dbcontext#implicitly-sharing-dbcontext-instances-across-multiple-threads-via-dependency-injection
            serviceCollection.Add(ServiceDescriptor.Describe(typeof(IEfRepository), serviceProvider => new EfRepository(context.Select(tuple =>
            {
                var profile = (IEntityProfile)ActivatorUtilities.CreateInstance(serviceProvider, tuple.ProfileType);

                return new EntityProfileAttributed
                {
                    EntityType = tuple.EntityType,
                    Profile = profile,
                    EntityMapping = profile.ToEntityMapping(entityTypes)
                };
            }).ToList(), serviceProvider.GetService<TDbContext>()), serviceLifetime));
            
            // Dependency inject IBasicCrud for each entity type
            foreach (var (_, entityType) in context)
            {
                serviceCollection.Add(ServiceDescriptor.Describe(typeof(IBasicCrud<>).MakeGenericType(entityType), serviceProvider =>
                {
                    var repository = serviceProvider.GetRequiredService<IEfRepository>();

                    return repository.For(entityType);
                }, serviceLifetime));
            }
        }

        private static Type GetProfileGenericType(Type t)
        {
            if (t.BaseType is { IsGenericType: true } && t.BaseType.GetGenericTypeDefinition() == typeof(EntityProfile<>))
            {
                return t.BaseType.GenericTypeArguments[0];
            }

            return null;
        }
    }
}