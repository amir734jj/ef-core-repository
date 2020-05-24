using System;
using System.Collections.Generic;
using System.Linq;
using EfCoreRepository.Interfaces;
using EfCoreRepository.Models;
using Microsoft.EntityFrameworkCore;

namespace EfCoreRepository
{
    internal class EfRepository : IEfRepository
    {
        private readonly DbContext _dbContext;
        
        private readonly IList<EntityProfileAttributed> _profiles;

        public EfRepository(IEnumerable<EntityProfileAttributed> profiles, DbContext dbContext)
        {
            _dbContext = dbContext;
            _profiles = profiles.ToList();
        }

        public IBasicCrud<TSource, TId> For<TSource, TId>() where TSource: class, IEntity<TId>
        {
            var profile = _profiles.FirstOrDefault(x => x.IdType == typeof(TId) && x.SourceType == typeof(TSource));

            if (profile == null)
            {
                throw new Exception($"Failed to find profile for {typeof(TSource).Name}<{typeof(TId).Name}>");
            }

            return new BasicCrud<TSource, TId>((IEntityProfile<TSource, TId>) profile.Profile, _dbContext);
        }
    }
}