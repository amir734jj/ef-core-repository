using System;

namespace Core.Models
{
    internal class EntityProfileAttributed
    {
        public Type SourceType { get; set; }
        
        public Type IdType { get; set; }
        
        public object Profile { get; set; }
    }
}