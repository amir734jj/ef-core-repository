using System.Linq;
using System.Security;
using ConsoleApp.Models;
using EfCoreRepository.Interfaces;

[assembly: SecurityRules(SecurityRuleSet.Level1, SkipVerificationInFullTrust = true)]
namespace ConsoleApp.Profiles
{
    public class NestedProfile : IEntityProfile<Nested>
    {
        public IQueryable<Nested> Include<TQueryable>(TQueryable queryable) where TQueryable : IQueryable<Nested>
        {
            return queryable;
        }
    }
}