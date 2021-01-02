using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfCoreRepository.Interfaces;
using EfCoreRepository.Models;
using Microsoft.EntityFrameworkCore;

namespace EfCoreRepository
{
    internal class EfRepository : IEfRepository, IEfRepositorySession
    {
        private readonly DbContext _dbContext;
        
        private readonly bool _session;

        private readonly IList<EntityProfileAttributed> _profiles;

        public EfRepository(IEnumerable<EntityProfileAttributed> profiles, DbContext dbContext, bool session)
        {
            _dbContext = dbContext;
            _session = session;
            _profiles = profiles.ToList();
        }

        public IEfRepositorySession Session()
        {
            return new EfRepository(_profiles, _dbContext, true);
        }

        public IBasicCrudWrapper<TSource> For<TSource>() where TSource: class, IUntypedEntity
        {
            var profile = _profiles.FirstOrDefault(x => x.SourceType == typeof(TSource));

            if (profile == null)
            {
                throw new Exception($"Failed to find profile for {typeof(TSource).Name}>");
            }

            return new BasicCrud<TSource>((IEntityProfile<TSource>) profile.Profile, _dbContext, _session);
        }

        IBasicCrud<TSource> IEfRepositorySession.For<TSource>()
        {
            return For<TSource>();
        }

        public ValueTask DisposeAsync()
        {
            if (_session)
            {
                return new ValueTask(_dbContext.SaveChangesAsync());
            }
            
            return new ValueTask(Task.CompletedTask);
        }

        public void Dispose()
        {
            if (_session)
            {
                _dbContext.Dispose();
            }
        }
    }
}