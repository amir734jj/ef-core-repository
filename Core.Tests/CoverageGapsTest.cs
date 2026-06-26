using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Tests.Abstracts;
using Core.Tests.Models;
using EfCoreRepository;
using EfCoreRepository.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Core.Tests;

/// <summary>
/// Targets behavior branches that the broader suite did not exercise - read-only join carrier,
/// the unsupported-join-type guard, split-query path, delayed update/delete, no-op deletes,
/// and both sync/async disposal of a delayed session.
/// </summary>
public class CoverageGapsTest : AbstractRepositoryTest
{
    [Fact]
    public void Joined_ExposesBothSides()
    {
        var pair = new Joined<DummyModel, NestedModel>
        {
            Outer = new DummyModel { Name = "outer" },
            Inner = new NestedModel { Id = 7 }
        };

        pair.Outer.Name.Should().Be("outer");
        pair.Inner.Id.Should().Be(7);
    }

    [Fact]
    public void Join_WithUnsupportedType_Throws()
    {
        var act = () => Repository.For<DummyModel>()
            .Join<NestedModel, int?>(d => d.Id, n => n.ParentRefId, (JoinType)999);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task SplitQuery_LoadsEntitiesWithChildren()
    {
        var parent = await Repository.For<DummyModel>().Save(new DummyModel { Name = "withKids", Children = [] });
        await Repository.For<NestedModel>().Save(new NestedModel { ParentRefId = parent.Id });

        var rows = (await Repository.For<DummyModel>()
            .SplitQuery()
            .GetAll<DummyModel>(filterExprs: [d => d.Name == "withKids"])).ToList();

        rows.Should().ContainSingle();
    }

    [Fact]
    public async Task DelayedSession_UpdateDefersUntilDispose()
    {
        var saved = await Repository.For<DummyModel>().Save(new DummyModel { Name = "before", Children = [] });

        var delayed = Repository.For<DummyModel>().Delayed();
        await delayed.Update(saved.Id, e => e.Name = "after");
        await delayed.DisposeAsync();

        (await Repository.For<DummyModel>().Get(saved.Id)).Name.Should().Be("after");
    }

    [Fact]
    public async Task DelayedSession_DeleteDefersUntilSyncDispose()
    {
        var saved = await Repository.For<DummyModel>().Save(new DummyModel { Name = "doomed", Children = [] });

        var delayed = Repository.For<DummyModel>().Delayed();
        await delayed.Delete(saved.Id);

        // Sync Dispose path also flushes pending changes.
        delayed.Dispose();

        (await Repository.For<DummyModel>().Get(saved.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_WithNoMatch_ReturnsNothing()
    {
        var result = await Repository.For<DummyModel>().Delete([d => d.Name == "does-not-exist"]);

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteMany_WithEmptyInputs_IsNoOp()
    {
        (await Repository.For<DummyModel>().DeleteMany([])).Should().BeEmpty();
        (await Repository.For<DummyModel>().DeleteMany<int>([])).Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_WithCustomIncludeExpression_LoadsChildren()
    {
        var parent = await Repository.For<DummyModel>().Save(new DummyModel { Name = "inc", Children = [] });
        await Repository.For<NestedModel>().Save(new NestedModel { ParentRefId = parent.Id });

        var rows = (await Repository.For<DummyModel>().GetAll<DummyModel>(
            filterExprs: [d => d.Name == "inc"],
            includeExprs: q => q.Include(d => d.Children))).ToList();

        rows.Should().ContainSingle();
        rows[0].Children.Should().ContainSingle();
    }

    [Fact]
    public async Task NoTracking_ReturnsResults()
    {
        await Repository.For<DummyModel>().Save(new DummyModel { Name = "untracked", Children = [] });

        var rows = await Repository.For<DummyModel>()
            .NoTracking()
            .GetAll<DummyModel>(filterExprs: [d => d.Name == "untracked"]);

        rows.Should().ContainSingle();
    }

    [Fact]
    public async Task HasReferences_LoadsCollectionNavigationWhenNotAlreadyLoaded()
    {
        var parent = await Repository.For<DummyModel>().Save(new DummyModel { Name = "lazyParent", Children = [] });
        await Repository.For<NestedModel>().Save(new NestedModel { ParentRefId = parent.Id });

        // Light() skips eager includes, so Children is unloaded and HasReferences must LoadAsync it.
        var reloaded = await Repository.For<DummyModel>().Light().Get([d => d.Id == parent.Id]);

        (await Repository.For<DummyModel>().HasReferences(reloaded)).Should().BeTrue();
    }
}
