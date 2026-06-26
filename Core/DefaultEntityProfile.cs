namespace EfCoreRepository
{
    /// <summary>
    /// The profile used for an entity that has no explicit <see cref="EntityProfile{TSource}"/>.
    /// It auto-maps every public read/write property (<c>MapAll</c>) and adds no eager includes,
    /// relying on the base class's no-op <c>Update</c>/<c>Include</c>. This is what makes profiles
    /// optional: call <c>DefaultProfiles()</c> at registration and every entity exposed by the
    /// DbContext gets one of these unless a profile is defined explicitly.
    /// </summary>
    internal sealed class DefaultEntityProfile<TEntity> : EntityProfile<TEntity> where TEntity : class
    {
        public DefaultEntityProfile()
        {
            MapAll();
        }
    }
}
