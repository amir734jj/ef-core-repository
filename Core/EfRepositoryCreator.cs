using System.Collections.Generic;
using System.Threading.Tasks;
using EfCoreRepository.Interfaces;
using EfCoreRepository.Models;
using Microsoft.EntityFrameworkCore;

namespace EfCoreRepository
{
    internal class EfRepositoryCreator<TDbContext, TSource>(
        IDbContextFactory<TDbContext> contextFactory,
        IEnumerable<EntityProfileAttributed> profiles)
        : IEfRepositoryCreator<TSource>
        where TDbContext : DbContext
        where TSource : class, new()
    {
        public IBasicCrud<TSource> Create()
        {
            var context = contextFactory.CreateDbContext();
            var repo = new EfRepository(profiles, context, ownsContext: true);
            return repo.ForInternal<TSource>(repo);
        }

        public async Task<IBasicCrud<TSource>> CreateAsync()
        {
            var context = await contextFactory.CreateDbContextAsync();
            var repo = new EfRepository(profiles, context, ownsContext: true);
            return repo.ForInternal<TSource>(repo);
        }
    }
}
