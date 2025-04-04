using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Tests.Models
{
    public sealed class NestedModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public DummyModel ParentRef { get; set; }
        
        public int? ParentRefId { get; set; }
    }
}