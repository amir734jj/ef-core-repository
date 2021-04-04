using System;
using System.Collections.Generic;
using System.Linq;
using EfCoreRepository.Interfaces;
using EfCoreRepository.Models;
using Microsoft.EntityFrameworkCore;
using static EfCoreRepository.Models.SessionType;

namespace EfCoreRepository
{
    internal class EfRepository : IEfRepository, IEfRepositorySession
    {
        private readonly DbContext _dbContext;
        
        private readonly IList<EntityProfileAttributed> _profiles;

        public EfRepository(IEnumerable<EntityProfileAttributed> profiles, DbContext dbContext)
        {
            _dbContext = dbContext;
            _profiles = profiles.ToList();
        }

        public IBasicCrudWrapper<TSource> For<TSource>() where TSource: class, IUntypedEntity
        {
            var profile = _profiles.FirstOrDefault(x => x.SourceType == typeof(TSource));

            if (profile == null)
            {
                throw new Exception($"Failed to find profile for {typeof(TSource).Name}>");
            }

            return new BasicCrud<TSource>((IEntityProfile<TSource>) profile.Profile, _dbContext, Generic);
        }

        IBasicCrud<TSource> IEfRepositorySession.For<TSource>()
        {
            return For<TSource>();
        }
    }
}