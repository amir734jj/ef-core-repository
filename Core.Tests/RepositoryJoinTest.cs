using System.Linq;
using System.Threading.Tasks;
using Core.Tests.Abstracts;
using Core.Tests.Models;
using EfCoreRepository.Models;
using FluentAssertions;
using Xunit;

namespace Core.Tests;

public class RepositoryJoinTest : AbstractRepositoryTest
{
    [Fact]
    public async Task Test_Join_Inner_ProjectsOnlyMatchingRows()
    {
        // Arrange
        var parent = await Repository.For<DummyModel>().Save(new DummyModel { Name = "Parent", Children = [] });
        var child = await Repository.For<NestedModel>().Save(new NestedModel { ParentRefId = parent.Id });

        // Orphan child with no matching parent — must be excluded by the inner join.
        await Repository.For<NestedModel>().Save(new NestedModel { ParentRefId = null });

        // Act
        var rows = (await Repository.For<DummyModel>()
            .Join<NestedModel, int?>(d => d.Id, n => n.ParentRefId, JoinType.Inner)
            .GetAll(
                filterExprs: [p => p.Outer.Name == "Parent"],
                orderBy: p => p.Inner.Id,
                project: p => new { p.Outer.Name, ChildId = p.Inner.Id })).ToList();

        // Assert
        rows.Should().HaveCount(1);
        rows[0].Name.Should().Be("Parent");
        rows[0].ChildId.Should().Be(child.Id);
    }

    [Fact]
    public async Task Test_Join_Left_IncludesRowsWithoutMatch()
    {
        // Arrange
        var withChild = await Repository.For<DummyModel>().Save(new DummyModel { Name = "HasChild", Children = [] });
        await Repository.For<DummyModel>().Save(new DummyModel { Name = "NoChild", Children = [] });
        await Repository.For<NestedModel>().Save(new NestedModel { ParentRefId = withChild.Id });

        // Act
        var rows = (await Repository.For<DummyModel>()
            .Join<NestedModel, int?>(d => d.Id, n => n.ParentRefId, JoinType.Left)
            .GetAll(
                orderBy: p => p.Outer.Name,
                project: p => new { p.Outer.Name, HasChild = p.Inner != null })).ToList();

        // Assert
        rows.Should().HaveCount(2);
        rows.Single(r => r.Name == "HasChild").HasChild.Should().BeTrue();
        rows.Single(r => r.Name == "NoChild").HasChild.Should().BeFalse();
    }

    [Fact]
    public async Task Test_Join_DefaultsToInner()
    {
        // Arrange
        var parent = await Repository.For<DummyModel>().Save(new DummyModel { Name = "P", Children = [] });
        await Repository.For<NestedModel>().Save(new NestedModel { ParentRefId = parent.Id });
        await Repository.For<DummyModel>().Save(new DummyModel { Name = "Lonely", Children = [] });

        // Act — omitting JoinType should behave as an inner join.
        var rows = await Repository.For<DummyModel>()
            .Join<NestedModel, int?>(d => d.Id, n => n.ParentRefId)
            .GetAll(project: p => new { p.Outer.Name });

        // Assert
        rows.Should().ContainSingle();
        rows.Single().Name.Should().Be("P");
    }

    [Fact]
    public async Task Test_Join_Right_IncludesUnmatchedInnerRows()
    {
        // Arrange
        var parent = await Repository.For<DummyModel>().Save(new DummyModel { Name = "Parent", Children = [] });
        await Repository.For<NestedModel>().Save(new NestedModel { ParentRefId = parent.Id });

        // Orphan child with no matching parent — a right join keeps it with a null outer.
        await Repository.For<NestedModel>().Save(new NestedModel { ParentRefId = null });

        // Act
        var rows = (await Repository.For<DummyModel>()
            .Join<NestedModel, int?>(d => d.Id, n => n.ParentRefId, JoinType.Right)
            .GetAll(project: p => new { OuterName = p.Outer != null ? p.Outer.Name : null, ChildId = p.Inner.Id })).ToList();

        // Assert — both children appear; the orphan has a null outer.
        rows.Should().HaveCount(2);
        rows.Should().ContainSingle(r => r.OuterName == "Parent");
        rows.Should().ContainSingle(r => r.OuterName == null);
    }

    [Fact]
    public async Task Test_Join_FullOuter_IncludesUnmatchedRowsFromBothSides()
    {
        // Arrange
        var matched = await Repository.For<DummyModel>().Save(new DummyModel { Name = "Matched", Children = [] });
        await Repository.For<DummyModel>().Save(new DummyModel { Name = "OuterOnly", Children = [] });
        await Repository.For<NestedModel>().Save(new NestedModel { ParentRefId = matched.Id });

        // Orphan child — no matching parent.
        await Repository.For<NestedModel>().Save(new NestedModel { ParentRefId = null });

        // Act
        var rows = (await Repository.For<DummyModel>()
            .Join<NestedModel, int?>(d => d.Id, n => n.ParentRefId, JoinType.FullOuter)
            .GetAll(project: p => new
            {
                OuterName = p.Outer != null ? p.Outer.Name : null,
                HasInner = p.Inner != null
            })).ToList();

        // Assert — matched pair, outer-only row, and inner-only row are all present.
        rows.Should().HaveCount(3);
        rows.Should().ContainSingle(r => r.OuterName == "Matched" && r.HasInner);
        rows.Should().ContainSingle(r => r.OuterName == "OuterOnly" && !r.HasInner);
        rows.Should().ContainSingle(r => r.OuterName == null && r.HasInner);
    }
}
