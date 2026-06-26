using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EfCoreRepository.Interfaces;
using EfCoreRepository.Models;

namespace EfCoreRepository.Extensions;

public static class BasicCurdExtension
{
    public static async Task<IEnumerable<TSource>> GetAll<TSource>(this IBasicCrud<TSource> basicCrud,
        Expression<Func<TSource, bool>>[] filterExprs = null,
        Func<IQueryable<TSource>, IQueryable<TSource>> includeExprs = null,
        Ordering<TSource> orderBy = null,
        int? skip = null,
        int? maxResults = null) where TSource : class, new()
    {
        return await basicCrud.GetAll<TSource>(filterExprs, includeExprs, orderBy, project: null, skip: skip, maxResults: maxResults);
    }
}