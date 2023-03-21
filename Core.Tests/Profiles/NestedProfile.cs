using System.Linq;
using Core.Tests.Models;
using EfCoreRepository;
using Microsoft.EntityFrameworkCore;

namespace Core.Tests.Profiles
{
    public class NestedProfile : EntityProfile<NestedModel>
    {
        public NestedProfile()
        {
            MapAll();
        }

        protected override IQueryable<NestedModel> Include<TQueryable>(TQueryable queryable)
        {
            return queryable.Include(x => x.ParentRef);
        }
    }
}