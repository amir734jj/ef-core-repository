namespace EfCoreRepository.Models
{
    /// <summary>
    /// The kind of SQL join to perform.
    /// </summary>
    public enum JoinType
    {
        /// <summary>Inner join — only rows with a match on both sides.</summary>
        Inner = 0,

        /// <summary>Left outer join — all outer rows; the inner side is <c>null</c> when unmatched.</summary>
        Left = 1,

        /// <summary>Right outer join — all inner rows; the outer side is <c>null</c> when unmatched.</summary>
        Right = 2,

        /// <summary>
        /// Full outer join — all rows from both sides; the unmatched side is <c>null</c>.
        /// Emitted as a <c>UNION ALL</c> of the left join and the unmatched-right rows.
        /// </summary>
        FullOuter = 3,
    }
}
