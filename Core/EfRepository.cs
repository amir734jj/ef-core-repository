using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using EfCoreRepository.Interfaces;
using EfCoreRepository.Models;
using Microsoft.EntityFrameworkCore;
using static EfCoreRepository.Models.SessionType;
using static EfCoreRepository.EntityUtility;

namespace EfCoreRepository
{
    internal class EfRepository : IEfRepository
    {
        private readonly DbContext _dbContext;
        
        private readonly IDictionary<Type, object> _profiles;

        public EfRepository(IEnumerable<EntityProfileAttributed> profiles, DbContext dbContext)
        {
            _dbContext = dbContext;
            _profiles = new ConcurrentDictionary<Type, object>(profiles.GroupBy(x => x.SourceType)
                .ToDictionary(x => x.Key, x => x.First().Profile));
        }

        public IBasicCrud<TSource> For<TSource>() where TSource: class
        {
            if (!_profiles.TryGetValue(typeof(TSource), out var profile))
            {
                throw new Exception($"Failed to find profile for {typeof(TSource).Name}>");
            }

            var keyProperty = FindIdProperty<TSource>();

            if (keyProperty == null)
            {
                throw new Exception("Missing Key attribute on entity");
            }

            return new BasicCrud<TSource>((EntityProfile<TSource>) profile, _dbContext, Generic);
        }
    }
}