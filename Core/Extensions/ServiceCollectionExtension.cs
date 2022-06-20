using System;
using EfCoreRepository.Interfaces;
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
    }
}