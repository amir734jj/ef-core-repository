using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace EfCoreRepository.Models
{
    /// <summary>Builds reusable predicates that EF Core can translate to SQL.</summary>
    public static class Filter<TSource>
    {
        private static readonly MethodInfo LikeMethod = typeof(DbFunctionsExtensions).GetMethod(
            nameof(DbFunctionsExtensions.Like),
            [typeof(DbFunctions), typeof(string), typeof(string)])!;

        /// <summary>Matches a string property using a SQL LIKE pattern.</summary>
        public static Expression<Func<TSource, bool>> Like(
            Expression<Func<TSource, string>> property,
            string pattern)
        {
            ArgumentNullException.ThrowIfNull(property);
            ArgumentNullException.ThrowIfNull(pattern);

            return BuildLikeAny(pattern, [property]);
        }

        /// <summary>Matches when any selected string property satisfies a SQL LIKE pattern.</summary>
        public static Expression<Func<TSource, bool>> LikeAny(
            string pattern,
            params Expression<Func<TSource, string>>[] properties)
        {
            ArgumentNullException.ThrowIfNull(pattern);
            ArgumentNullException.ThrowIfNull(properties);
            if (properties.Length == 0)
            {
                throw new ArgumentException("At least one property selector is required.", nameof(properties));
            }

            return BuildLikeAny(pattern, properties);
        }

        private static Expression<Func<TSource, bool>> BuildLikeAny(
            string pattern,
            IReadOnlyCollection<Expression<Func<TSource, string>>> properties)
        {
            var parameter = Expression.Parameter(typeof(TSource), "source");
            var patternExpression = Expression.Constant(pattern);
            var functions = Expression.Property(null, typeof(EF), nameof(EF.Functions));
            var matches = properties.Select<Expression<Func<TSource, string>>, Expression>(property =>
            {
                ArgumentNullException.ThrowIfNull(property);
                var value = new ParameterReplaceVisitor(property.Parameters[0], parameter).Visit(property.Body)!;
                return Expression.Call(LikeMethod, functions, value, patternExpression);
            });

            var body = matches.Aggregate(Expression.OrElse);
            return Expression.Lambda<Func<TSource, bool>>(body, parameter);
        }

        private sealed class ParameterReplaceVisitor(ParameterExpression source, ParameterExpression replacement) : ExpressionVisitor
        {
            protected override Expression VisitParameter(ParameterExpression node) =>
                node == source ? replacement : base.VisitParameter(node);
        }
    }
}