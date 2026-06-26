namespace EfCoreRepository.Models
{
    /// <summary>
    /// The kind of SQL join to perform.
    /// </summary>
    public enum JoinType
    {
        /// <summary>
        /// Inner join - only rows with a match on both sides.
        /// In plain terms: keep only the rows that appear on <em>both</em> lists; anything without a partner is dropped.
        /// </summary>
        Inner = 0,

        /// <summary>
        /// Left outer join - all outer rows; the inner side is <c>null</c> when unmatched.
        /// In plain terms: keep every row from the left side; if there's no match on the right, the right side comes back empty.
        /// </summary>
        Left = 1,

        /// <summary>
        /// Right outer join - all inner rows; the outer side is <c>null</c> when unmatched.
        /// In plain terms: keep every row from the right side; if there's no match on the left, the left side comes back empty.
        /// </summary>
        Right = 2,

        /// <summary>
        /// Full outer join - all rows from both sides; the unmatched side is <c>null</c>.
        /// Emitted as a <c>UNION ALL</c> of the left join and the unmatched-right rows.
        /// In plain terms: keep everything from both sides; pair up rows that match, and still show rows that have no partner with the missing side left empty.
        /// </summary>
        FullOuter = 3,
    }
}
