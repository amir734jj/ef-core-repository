using System;
using System.Collections.Generic;
using EfCoreRepository.Interfaces;
using EfCoreRepository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EfCoreRepository.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddEfRepository<TDbContext>(this IServiceCollection collection, Action<IEfRepositoryFactory> options, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped) where TDbContext : DbContext
        {
            var factory = new EfRepositoryFactory<TDbContext>(collection, serviceLifetime);

            options(factory);
            
            factory.Build();
            
            return collection;
        }

        /// <summary>
        /// Registers IEfRepositoryCreator backed by IDbContextFactory, enabling parallel query execution.
        /// Requires IDbContextFactory&lt;TDbContext&gt; to be registered (via AddDbContextFactory).
        /// Also registers scoped IEfRepository and IBasicCrud&lt;T&gt; for standard (non-parallel) usage.
        /// </summary>
        public static IServiceCollection AddEfRepositoryFactory<TDbContext>(this IServiceCollection collection, Action<IEfRepositoryFactory> options) where TDbContext : DbContext
        {
            // Register profiles and scoped IEfRepository / IBasicCrud<T> (standard usage)
            var factory = new EfRepositoryFactory<TDbContext>(collection, ServiceLifetime.Scoped);

            options(factory);
            
            factory.Build();

            // Register IEfRepositoryCreator<T> for each entity type
            foreach (var entityType in factory.EntityTypes)
            {
                var creatorInterface = typeof(IEfRepositoryCreator<>).MakeGenericType(entityType);
                var creatorImpl = typeof(EfRepositoryCreator<,>).MakeGenericType(typeof(TDbContext), entityType);

                collection.AddSingleton(creatorInterface, sp =>
                    Activator.CreateInstance(creatorImpl,
                        sp.GetRequiredService<IDbContextFactory<TDbContext>>(),
                        sp.GetRequiredService<IEnumerable<EntityProfileAttributed>>()));
            }

            return collection;
        }
    }
}