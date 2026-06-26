using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EfCoreRepository.Interfaces
{
    public interface IBasicCrudUtils<TSource> where TSource : class
    {
        Task<bool> HasReferences(TSource source);
    }
}