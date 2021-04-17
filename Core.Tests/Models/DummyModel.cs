using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Core.Tests.Models
{
    public class DummyModel
    {
        [Key]
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public virtual List<Nested> Children { get; set; }
    }
}