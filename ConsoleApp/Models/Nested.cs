using Core.Interfaces;

namespace ConsoleApp.Models
{
    public class Nested : IEntity<int>
    {
        public int Id { get; set; }
    }
}