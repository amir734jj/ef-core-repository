using System;

namespace EfCoreRepository.Models
{
    internal class EntityProfileAttributed
    {
        public Type SourceType { get; set; }

        public object Profile { get; set; }
    }
}