using System.Linq;
using Core.Tests.Models;
using EfCoreRepository;
using EfCoreRepository.Interfaces;

namespace Core.Tests.Profiles
{
    public class NestedProfile : EntityProfile<Nested>
    {
        public override void Update(Nested entity, Nested dto)
        {
            // No change
        }

        public override IQueryable<Nested> Include<TQueryable>(TQueryable queryable)
        {
            return queryable;
        }
    }
}