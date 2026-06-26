using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EfCoreRepository.Models
{
    /// <summary>
    /// A fluent, immutable multi-key sort specification. Start with <see cref="Asc"/> or
    /// <see cref="Desc"/>, then chain <see cref="ThenAsc"/> / <see cref="ThenDesc"/> to add
    /// secondary keys. The first key becomes <c>ORDER BY</c> and each subsequent key a
    /// <c>THEN BY</c>, preserving direction per key. Each chaining call returns a new instance.
    /// </summary>
    public sealed class Ordering<TSource>
    {
        private readonly IReadOnlyList<(Expression<Func<TSource, object>> KeySelector, bool Descending)> _keys;

        private Ordering(IReadOnlyList<(Expression<Func<TSource, object>> KeySelector, bool Descending)> keys)
        {
            _keys = keys;
        }

        // The ordered sort keys, consumed by the query layer in the same assembly.
        internal IReadOnlyList<(Expression<Func<TSource, object>> KeySelector, bool Descending)> Keys => _keys;

        /// <summary>Starts an ascending sort by <paramref name="keySelector"/>.</summary>
        public static Ordering<TSource> Asc(Expression<Func<TSource, object>> keySelector) =>
            new([(keySelector, false)]);

        /// <summary>Starts a descending sort by <paramref name="keySelector"/>.</summary>
        public static Ordering<TSource> Desc(Expression<Func<TSource, object>> keySelector) =>
            new([(keySelector, true)]);

        /// <summary>Adds an ascending secondary sort key.</summary>
        public Ordering<TSource> ThenAsc(Expression<Func<TSource, object>> keySelector) =>
            Append(keySelector, false);

        /// <summary>Adds a descending secondary sort key.</summary>
        public Ordering<TSource> ThenDesc(Expression<Func<TSource, object>> keySelector) =>
            Append(keySelector, true);

        private Ordering<TSource> Append(Expression<Func<TSource, object>> keySelector, bool descending)
        {
            var keys = new List<(Expression<Func<TSource, object>> KeySelector, bool Descending)>(_keys) { (keySelector, descending) };
            return new Ordering<TSource>(keys);
        }
    }
}
