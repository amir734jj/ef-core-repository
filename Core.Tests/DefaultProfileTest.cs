using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using EfCoreRepository.Extensions;
using EfCoreRepository.Interfaces;
using EfCoreRepository.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Core.Tests;

/// <summary>
/// Verifies that profiles are optional: an entity registered only through
/// <c>DefaultProfiles()</c> — with no hand-written <c>EntityProfile&lt;T&gt;</c> — still
/// supports CRUD and joins via an auto-generated MapAll/no-include profile.
/// </summary>
public class DefaultProfileTest
{
    // Note: no EntityProfile<T> is defined for either of these.
    private sealed class OrphanParent
    {
        [Key] public int Id { get; set; }
        public string Name { get; set; }
    }

    private sealed class OrphanChild
    {
        [Key] public int Id { get; set; }
        public int ParentId { get; set; }
    }

    private sealed class OrphanDbContext(DbContextOptions<OrphanDbContext> options) : DbContext(options)
    {
        public DbSet<OrphanParent> Parents { get; set; }
        public DbSet<OrphanChild> Children { get; set; }
    }

    // No [Key] and no id/_id property: not a discoverable key.
    private sealed class KeylessEntity
    {
        public string Name { get; set; }
    }

    private sealed class KeylessDbContext(DbContextOptions<KeylessDbContext> options) : DbContext(options)
    {
        public DbSet<KeylessEntity> Keyless { get; set; }
    }

    private static IEfRepository BuildRepository()
    {
        var provider = new ServiceCollection()
            .AddDbContext<OrphanDbContext>(x => x.UseSqlite("DataSource=file:orphandb?mode=memory&cache=shared"))
            .AddEfRepository<OrphanDbContext>(options => options
                .DefaultProfiles())
            .BuildServiceProvider();

        // Create the schema (and keep the shared in-memory database alive via the scoped context).
        provider.GetRequiredService<OrphanDbContext>().Database.EnsureCreated();

        return provider.GetRequiredService<IEfRepository>();
    }

    [Fact]
    public async Task EntityWithoutProfile_SupportsCrud()
    {
        var repo = BuildRepository();

        var saved = await repo.For<OrphanParent>().Save(new OrphanParent { Name = "no-profile" });
        var fetched = await repo.For<OrphanParent>().Get([p => p.Id == saved.Id]);

        fetched.Should().NotBeNull();
        fetched.Name.Should().Be("no-profile");
    }

    [Fact]
    public async Task EntitiesWithoutProfile_SupportJoin()
    {
        var repo = BuildRepository();

        var parent = await repo.For<OrphanParent>().Save(new OrphanParent { Name = "Parent" });
        await repo.For<OrphanChild>().Save(new OrphanChild { ParentId = parent.Id });

        var rows = (await repo.For<OrphanParent>()
            .Join<OrphanChild, int>(p => p.Id, c => c.ParentId, JoinType.Inner)
            .GetAll(project: pair => new { pair.Outer.Name, ChildId = pair.Inner.Id })).ToList();

        rows.Should().ContainSingle();
        rows[0].Name.Should().Be("Parent");
    }

    [Fact]
    public void DefaultProfiles_WithKeylessEntity_ThrowsAtRegistration()
    {
        // A DbContext entity with no discoverable key must fail fast at registration, not crash
        // later when the repository is first used.
        Action act = () => new ServiceCollection()
            .AddDbContext<KeylessDbContext>(x => x.UseSqlite("DataSource=file:keylessdb?mode=memory&cache=shared"))
            .AddEfRepository<KeylessDbContext>(options => options.DefaultProfiles());

        act.Should().Throw<Exception>().WithMessage("*Missing key identifier*KeylessEntity*");
    }
}
