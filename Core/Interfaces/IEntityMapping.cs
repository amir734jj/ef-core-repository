using System.Linq;

namespace EfCoreRepository.Interfaces
{
    public interface IEntityMapping
    {
        public void Update(object entityUntyped, object dtoUntyped);
        
        public IQueryable Include(IQueryable queryableUntyped);
    }
}