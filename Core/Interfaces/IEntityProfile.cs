using System;
using System.Collections.Generic;

namespace EfCoreRepository.Interfaces
{
    public interface IEntityProfile
    {
        IEntityMapping ToEntityMapping(IList<Type> entityTypes);
    }
}