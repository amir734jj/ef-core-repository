using System.Collections.Generic;
using Core.Interfaces;

namespace ConsoleApp.Models
{
    public class DummyModel : IEntity<int>
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public List<Nested> Children { get; set; }
    }
}