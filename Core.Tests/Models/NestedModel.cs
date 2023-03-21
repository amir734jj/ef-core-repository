using System.ComponentModel.DataAnnotations;

namespace Core.Tests.Models
{
    public class NestedModel
    {
        [Key]
        public int Id { get; set; }
        
        public virtual DummyModel ParentRef { get; set; }
        
        public int? ParentRefId { get; set; }
    }
}