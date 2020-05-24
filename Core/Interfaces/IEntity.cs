using System.ComponentModel.DataAnnotations;

namespace EfCoreRepository.Interfaces
{
    public interface IEntity<TId>
    {
        [Key]
        public TId Id { get; set; }
    }
}