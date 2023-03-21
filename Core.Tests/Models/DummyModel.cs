using System.Collections.Generic;

namespace Core.Tests.Models
{
    public class DummyModel
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public virtual List<NestedModel> Children { get; set; }
    }
}