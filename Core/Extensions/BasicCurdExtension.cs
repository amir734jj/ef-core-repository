using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EfCoreRepository.Interfaces;

namespace EfCoreRepository.Extensions;

public static class BasicCurdExtension
{
    public static async Task<IEnumerable<TSource>> GetAll<TSource>(this IBasicCrud<TSource> basicCrud,
        Expression<Func<TSource, bool>>[] filterExprs = null,
        Func<IQueryable<TSource>, IQueryable<TSource>> includeExprs = null,
        Expression<Func<TSource, object>> orderBy = null,
        Expression<Func<TSource, object>> orderByDesc = null,
        int? maxResults = null) where TSource : class, new()
    {
        return await basicCrud.GetAll<TSource>(filterExprs, includeExprs, orderBy, orderByDesc, project: null, maxResults);
    }
}