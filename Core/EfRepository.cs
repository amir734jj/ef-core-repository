using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EfCoreRepository.Interfaces;
using EfCoreRepository.Models;
using Microsoft.EntityFrameworkCore;
using static EfCoreRepository.Models.SessionType;
using static EfCoreRepository.EntityUtility;

namespace EfCoreRepository
{
    internal class EfRepository : IEfRepositorySession
    {
        private readonly DbContext _dbContext;
        private readonly bool _ownsContext;
        
        private readonly IDictionary<Type, EntityProfileAttributed> _profiles;

        public EfRepository(IEnumerable<EntityProfileAttributed> profiles, DbContext dbContext, bool ownsContext = false)
        {
            _dbContext = dbContext;
            _ownsContext = ownsContext;
            _profiles = new ConcurrentDictionary<Type, EntityProfileAttributed>(
                profiles.GroupBy(x => x.EntityType)
                .ToDictionary(x => x.Key, x => x.First()));
        }

        public IBasicCrud<TSource> For<TSource>() where TSource : class, new()
        {
            return ForInternal<TSource>(null);
        }

        internal IBasicCrud<TSource> ForInternal<TSource>(IAsyncDisposable ownedSession) where TSource : class, new()
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

            return new BasicCrud<TSource>(profile.EntityMapping, _dbContext, Generic, ownedSession);
        }

        object IEfRepository.For(Type type)
        {
            // ensure T is class and has parameterless constructor
            if (type.IsClass && type.GetConstructor(Type.EmptyTypes) != null)
            {
                return GetType().GetMethod(nameof(For), BindingFlags.Public | BindingFlags.Instance)!.MakeGenericMethod(type).Invoke(this, null);   
            }
            
            return null;
        }

        public void Dispose()
        {
            if (_ownsContext) _dbContext.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (_ownsContext) await _dbContext.DisposeAsync();
        }
    }
}