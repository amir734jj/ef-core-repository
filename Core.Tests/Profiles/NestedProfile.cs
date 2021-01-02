using System.Linq;
using Core.Tests.Models;
using EfCoreRepository.Interfaces;

namespace Core.Tests.Profiles
{
    public class NestedProfile : IEntityProfile<Nested>
    {
        public void Update(Nested entity, Nested dto)
        {
            // No change
        }

        public IQueryable<Nested> Include<TQueryable>(TQueryable queryable) where TQueryable : IQueryable<Nested>
        {
            return queryable;
        }
    }
}