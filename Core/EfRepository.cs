using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        
        private readonly IDictionary<Type, EntityProfileAttributed> _profiles;

        public EfRepository(IEnumerable<EntityProfileAttributed> profiles, DbContext dbContext)
        {
            _dbContext = dbContext;
            _profiles = new ConcurrentDictionary<Type, EntityProfileAttributed>(
                profiles.GroupBy(x => x.EntityType)
                .ToDictionary(x => x.Key, x => x.First()));
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
                throw new Exception($"Missing primary key identifier in entity {typeof(TSource).Name}");
            }

            return new BasicCrud<TSource>(profile.EntityMapping, _dbContext, Generic);
        }

        object IEfRepository.For(Type type)
        {
            return GetType().GetMethod(nameof(For), BindingFlags.Public | BindingFlags.Instance)!.MakeGenericMethod(type).Invoke(this, null);
        }
    }
}