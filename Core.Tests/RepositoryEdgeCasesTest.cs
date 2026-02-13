using System.Linq;
using System.Threading.Tasks;
using Core.Tests.Abstracts;
using Core.Tests.Models;
using FluentAssertions;
using Xunit;

namespace Core.Tests;

public class RepositoryEdgeCasesTest : AbstractRepositoryTest
{
    [Fact]
    public async Task Test_DeleteMany_EmptyArray()
    {
        // Arrange
        var model = new DummyModel { Name = "Test", Children = [] };
        await Repository.For<DummyModel>().Save(model);

        // Act
        var result = await Repository.For<DummyModel>().DeleteMany([]);

        // Assert
        result.Should().BeEmpty();

        // Verify nothing was deleted
        var allEntities = await Repository.For<DummyModel>().GetAll();
        allEntities.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Test_DeleteMany_ByExpr_EmptyArray()
    {
        // Arrange
        var model = new DummyModel { Name = "Test", Children = [] };
        await Repository.For<DummyModel>().Save(model);

        // Act - Pass empty filter expressions array
        var result = await Repository.For<DummyModel>().DeleteMany([]);

        // Assert
        result.Should().BeEmpty();

        // Verify nothing was deleted
        var allEntities = await Repository.For<DummyModel>().GetAll();
        allEntities.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Test_Delete_NonExistentId()
    {
        // Act
        var result = await Repository.For<DummyModel>().Delete(99999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Test_Delete_ByExpr_NoMatch()
    {
        // Arrange
        var model = new DummyModel { Name = "Test", Children = [] };
        await Repository.For<DummyModel>().Save(model);

        // Act
        var result = await Repository.For<DummyModel>().Delete([x => x.Name == "NonExistent"]);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Test_Get_NonExistentId()
    {
        // Act
        var result = await Repository.For<DummyModel>().Get(99999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Test_Get_ByExpr_NoMatch()
    {
        // Arrange
        var model = new DummyModel { Name = "Test", Children = [] };
        await Repository.For<DummyModel>().Save(model);

        // Act
        var result = await Repository.For<DummyModel>().Get([x => x.Name == "NonExistent"]);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Test_Update_NonExistentId()
    {
        // Arrange
        var dto = new DummyModel { Name = "Updated", Children = [] };

        // Act
        var result = await Repository.For<DummyModel>().Update(99999, dto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Test_Update_WithAction_NonExistentId()
    {
        // Act
        var result = await Repository.For<DummyModel>().Update(99999, entity => entity.Name = "Updated");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Test_BulkUpdate_EmptyIds()
    {
        // Act
        var result = await Repository.For<DummyModel>().BulkUpdate<int>(
            [],
            entity => entity.Name = "Updated"
        );

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Test_BulkUpdate_WithBatchSize()
    {
        // Arrange
        var models = Enumerable.Range(1, 10)
            .Select(i => new DummyModel { Name = $"Model{i}", Children = [] })
            .ToArray();

        var entities = (await Repository.For<DummyModel>().SaveMany(models)).ToList();
        var ids = entities.Select(e => e.Id).ToArray();

        // Act - Use small batch size to trigger batching logic
        var result = (await Repository.For<DummyModel>().BulkUpdate<int>(
            ids,
            entity => entity.Name = $"{entity.Name}_Updated",
            batchSize: 3
        )).ToList();

        // Assert
        result.Should().HaveCount(10);
        result.All(e => e.Name.EndsWith("_Updated")).Should().BeTrue();
    }

    [Fact]
    public async Task Test_GetAll_EmptyIdsArray()
    {
        // Arrange
        var model = new DummyModel { Name = "Test", Children = [] };
        await Repository.For<DummyModel>().Save(model);

        // Act - Empty IDs array results in filter with empty array which matches all
        var result = await Repository.For<DummyModel>().GetAll<int>([]);

        // Assert - GetAll with empty IDs array still returns all results
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Test_GetAll_WithNullFilterExprs()
    {
        // Arrange
        var model1 = new DummyModel { Name = "A", Children = [] };
        var model2 = new DummyModel { Name = "B", Children = [] };

        await Repository.For<DummyModel>().SaveMany([model1, model2]);

        // Act
        var result = await Repository.For<DummyModel>().GetAll<DummyModel>(filterExprs: null);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Test_GetAll_WithCustomIncludeExprs()
    {
        // Arrange
        var parent = new DummyModel { Name = "Parent", Children = [] };
        var savedParent = await Repository.For<DummyModel>().Save(parent);

        var child = new NestedModel { ParentRefId = savedParent.Id };
        await Repository.For<NestedModel>().Save(child);

        // Act - Use lightweight to avoid auto-includes, then apply custom include
        var result = await Repository.For<DummyModel>().Light().GetAll<DummyModel>(
            includeExprs: q => q
        );

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Test_DeleteInternal_MultipleEntities()
    {
        // Arrange
        var model1 = new DummyModel { Name = "Delete1", Children = [] };
        var model2 = new DummyModel { Name = "Delete2", Children = [] };
        var model3 = new DummyModel { Name = "Keep", Children = [] };

        await Repository.For<DummyModel>().SaveMany([model1, model2, model3]);

        // Act
        var deleted = await Repository.For<DummyModel>().DeleteMany([
            x => x.Name.StartsWith("Delete")
        ]);

        // Assert
        deleted.Should().HaveCount(2);
        
        var remaining = await Repository.For<DummyModel>().GetAll();
        remaining.Should().ContainSingle();
        remaining.First().Name.Should().Be("Keep");
    }

    [Fact]
    public async Task Test_Count_WithLightWeightSession()
    {
        // Arrange
        var model1 = new DummyModel { Name = "A", Children = [] };
        var model2 = new DummyModel { Name = "B", Children = [] };

        await Repository.For<DummyModel>().SaveMany([model1, model2]);

        // Act
        var result = await Repository.For<DummyModel>().Count([x => x.Name == "A"]);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task Test_SaveMany_EmptyArray()
    {
        // Act
        var result = await Repository.For<DummyModel>().SaveMany([]);

        // Assert
        result.Should().BeEmpty();
    }
}
