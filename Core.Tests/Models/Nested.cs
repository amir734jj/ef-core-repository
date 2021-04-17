using System.ComponentModel.DataAnnotations;

namespace Core.Tests.Models
{
    public class Nested
    {
        [Key]
        public int Id { get; set; }
        
        public virtual DummyModel ParentRef { get; set; }
    }
}