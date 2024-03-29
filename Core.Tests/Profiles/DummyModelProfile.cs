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
    }
}