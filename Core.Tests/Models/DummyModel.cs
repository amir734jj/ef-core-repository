using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Tests.Models
{
    public class DummyModel
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public virtual List<Nested> Children { get; set; }
    }
}