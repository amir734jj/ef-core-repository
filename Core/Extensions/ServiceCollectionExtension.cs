using System;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddEfRepository<TDbContext>(this IServiceCollection collection, Action<IEfRepositoryFactory> options) where TDbContext : DbContext
        {
            var factory = new EfRepositoryFactory(collection);

            options(factory);
            
            factory.Build<TDbContext>();
            
            return collection;
        }
    }
}