using System;
using EfCoreRepository.Interfaces;

namespace EfCoreRepository.Models
{
    internal class EntityProfileAttributed
    {
        public Type EntityType { get; set; }

        public IEntityProfile Profile { get; set; }
        
        public IEntityMapping EntityMapping { get; set; }
    }
}