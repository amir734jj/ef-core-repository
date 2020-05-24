using System.ComponentModel.DataAnnotations;

namespace Core.Interfaces
{
    public interface IEntity<TId>
    {
        [Key]
        public TId Id { get; set; }
    }
}