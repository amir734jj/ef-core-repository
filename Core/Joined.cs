namespace EfCoreRepository
{
    /// <summary>
    /// A joined row carrying both sides of a join. For a
    /// <see cref="Models.JoinType.Left"/> join, <see cref="Inner"/> is <c>null</c>
    /// when the outer row has no match.
    /// </summary>
    /// <typeparam name="TOuter">The outer (driving) entity.</typeparam>
    /// <typeparam name="TInner">The inner (joined) entity.</typeparam>
    public sealed class Joined<TOuter, TInner>
    {
        public TOuter Outer { get; init; }

        public TInner Inner { get; init; }
    }
}
