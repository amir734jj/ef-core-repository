using System.Collections.Generic;
using EfCoreRepository.Interfaces;

namespace Core.Tests.Models
{
    public class DummyModel : IEntity<int>
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public virtual List<Nested> Children { get; set; }
    }
}