using System.ComponentModel.DataAnnotations;

namespace EfCoreRepository.Interfaces
{
    public interface IEntity<TId> : IUntypedEntity
    {
        [Key]
        public TId Id { get; set; }
    }

    public interface IUntypedEntity
    {
        
    }
}