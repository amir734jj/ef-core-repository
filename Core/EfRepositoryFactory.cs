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

        private bool _useDefaultProfiles;

        internal IReadOnlyList<Type> EntityTypes => _context.Select(x => x.EntityType).ToList();

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

        public IEfRepositoryFactory DefaultProfiles()
        {
            _useDefaultProfiles = true;

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
            AddDefaultProfiles();

            AddEfRepository(_context);
        }

        // When enabled, registers a DefaultEntityProfile<T> (MapAll + no include) for every entity
        // type exposed by the DbContext that has no explicit profile. Default profiles are a last
        // resort, so an explicitly registered profile always wins. Fails fast if any DbContext
        // entity lacks a discoverable key, rather than silently skipping it and crashing at runtime.
        private void AddDefaultProfiles()
        {
            if (!_useDefaultProfiles)
            {
                return;
            }

            var entityTypes = DiscoverDbContextEntityTypes().ToList();

            var keyless = entityTypes.Where(t => !HasDiscoverableKey(t)).ToList();
            if (keyless.Count != 0)
            {
                throw new Exception(
                    $"Missing key identifier for DbContext entities: {string.Join(", ", keyless.Select(t => t.Name))}");
            }

            var alreadyProfiled = _context.Select(x => x.EntityType).ToHashSet();

            var defaults = entityTypes
                .Where(alreadyProfiled.Add) // also dedupes repeated DbSet types
                .Select(entityType => (
                    ProfileType: typeof(DefaultEntityProfile<>).MakeGenericType(entityType),
                    EntityType: entityType));

            _context.AddRange(defaults);
        }

        // Entity CLR types exposed as DbSet<T> properties on the context.
        private static IEnumerable<Type> DiscoverDbContextEntityTypes()
        {
            return typeof(TDbContext).GetProperties()
                .Where(p => p.PropertyType.IsGenericType
                            && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .Select(p => p.PropertyType.GetGenericArguments()[0]);
        }

        private static bool HasDiscoverableKey(Type type)
        {
            try
            {
                FindIdProperty(type);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void AddEfRepository(IReadOnlyCollection<(Type ProfileType, Type EntityType)> context)
        {
            var entityTypes = context.Select(x => x.EntityType).ToList();
            var duplicateProfiles =  context.GroupBy(x => x.EntityType).Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .ToList();

            // Check for duplicate profiles
            if (duplicateProfiles.Count != 0)
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
            if (missingKeys.Count != 0)
            {
                throw new Exception($"Missing KEY attribute on the class declaration for entities: {string.Join(", ", missingKeys.Select(x => x.EntityType.Name))}");
            }

            serviceCollection.Add(
                ServiceDescriptor.Describe(
                    typeof(IEnumerable<EntityProfileAttributed>),
                    serviceProvider =>
                    {
                        return context.Select(tuple =>
                        {
                            var profile = (IEntityProfile)ActivatorUtilities.CreateInstance(serviceProvider, tuple.ProfileType);
                            return new EntityProfileAttributed
                            {
                                EntityType = tuple.EntityType,
                                Profile = profile,
                                EntityMapping = profile.ToEntityMapping(entityTypes)
                            };
                        }).ToList();
                    },
                    ServiceLifetime.Singleton));

            // DbContext is registered by default as scoped
            // https://docs.microsoft.com/en-us/ef/core/miscellaneous/configuring-dbcontext#implicitly-sharing-dbcontext-instances-across-multiple-threads-via-dependency-injection
            serviceCollection.Add(ServiceDescriptor.Describe(
                typeof(IEfRepository),
                serviceProvider =>
                    new EfRepository(
                        serviceProvider.GetRequiredService<IEnumerable<EntityProfileAttributed>>(),
                        serviceProvider.GetRequiredService<TDbContext>()),
                serviceLifetime));

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