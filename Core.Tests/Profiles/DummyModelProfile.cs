using System;
using System.Collections.Generic;
using System.Linq;
using Core.Tests.Models;
using EfCoreRepository;
using Microsoft.EntityFrameworkCore;

namespace Core.Tests.Profiles
{
    public class DummyModelProfile : EntityProfile<DummyModel> 
    {
        public DummyModelProfile()
        {
            MapAll();
        }

        protected override IQueryable<DummyModel> Include<TQueryable>(TQueryable queryable)
        {
            return queryable.Include(x => x.Children);
        }

        // Test helper to expose ModifyList for testing
        public static void TestModifyList<TProperty, TId>(IList<TProperty> entity, IList<TProperty> dto, Func<TProperty, TId> idSelector)
            where TProperty : class
            where TId : struct
        {
            ModifyList(entity, dto, idSelector);
        }
    }
}
