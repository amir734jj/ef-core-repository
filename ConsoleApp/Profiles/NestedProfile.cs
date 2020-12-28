using System.Linq;
using ConsoleApp.Models;
using EfCoreRepository.Interfaces;

namespace ConsoleApp.Profiles
{
    public class NestedProfile : IEntityProfile<Nested, int>
    {
        public IQueryable<Nested> Include<TQueryable>(TQueryable queryable) where TQueryable : IQueryable<Nested>
        {
            return queryable;
        }
    }
}