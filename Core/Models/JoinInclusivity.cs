namespace EfCoreRepository.Models
{
    /// <summary>
    /// Whether a join keeps matched rows or only the rows that exist on a single side.
    /// This is the "filled lens" vs. "outer crescents" distinction from the classic join
    /// Venn diagram, kept separate from <see cref="JoinType"/> so the two choices compose
    /// cleanly instead of multiplying into one enum with awkward names.
    /// </summary>
    public enum JoinInclusivity
    {
        /// <summary>
        /// Keep every row the join produces, matched and unmatched alike.
        /// In plain terms: the normal join - the whole shaded region of the diagram.
        /// </summary>
        Inclusive = 0,

        /// <summary>
        /// Keep only the rows that exist on a single side (drop everything that matched on both).
        /// In plain terms: just the outer crescents - <c>not B</c> for a left join, <c>not A</c> for a
        /// right join, and the symmetric difference <c>A XOR B</c> for a full outer join.
        /// </summary>
        Exclusive = 1,
    }
}
