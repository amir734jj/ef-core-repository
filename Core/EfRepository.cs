using System;
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
        private readonly List<EntityProfileAttributed> _profiles;

        public EfRepository(List<EntityProfileAttributed> profiles)
        {
            _profiles = profiles;
        }

        public IBasicCrud<TSource> For<TSource>(DbContext context) where TSource: class
        {
            var profile = _profiles.FirstOrDefault(x => x.SourceType == typeof(TSource));

            if (profile == null)
            {
                throw new Exception($"Failed to find profile for {typeof(TSource).Name}>");
            }

            var keyProperty = FindIdProperty<TSource>();

            if (keyProperty == null)
            {
                throw new Exception("Missing Key attribute on entity");
            }

            return new BasicCrud<TSource>((EntityProfile<TSource>) profile.Profile, context, Generic);
        }
    }
}