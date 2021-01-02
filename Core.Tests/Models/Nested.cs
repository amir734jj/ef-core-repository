using EfCoreRepository.Interfaces;

namespace Core.Tests.Models
{
    public class Nested : IEntity<int>
    {
        public int Id { get; set; }
    }
}