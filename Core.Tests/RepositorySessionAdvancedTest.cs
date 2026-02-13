using System.Linq;
using System.Threading.Tasks;
using Core.Tests.Abstracts;
using Core.Tests.Models;
using FluentAssertions;
using Xunit;

namespace Core.Tests;

public class RepositorySessionAdvancedTest : AbstractRepositoryTest
{
    [Fact]
    public async Task Test_LightWeightSession()
    {
        // Arrange
        var parent = new DummyModel { Name = "Parent", Children = [] };
        var savedParent = await Repository.For<DummyModel>().Save(parent);

        var child = new NestedModel { ParentRefId = savedParent.Id };
        await Repository.For<NestedModel>().Save(child);

        // Act - Use NoTracking to verify lightweight behavior (prevents auto-includes)
        var lightRepository = Repository.For<DummyModel>().Light().NoTracking();
        var result = await lightRepository.Get(savedParent.Id);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Parent");
    }

    [Fact]
    public async Task Test_NoTrackingSession()
    {
        // Arrange
        var model = new DummyModel { Name = "Test", Children = [] };
        var entity = await Repository.For<DummyModel>().Save(model);

        // Act
        var repository = Repository.For<DummyModel>().NoTracking();
        var result = await repository.Get(entity.Id);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test");
    }

    [Fact]
    public async Task Test_SplitQuerySession()
    {
        // Arrange
        var parent = new DummyModel { Name = "Parent", Children = [] };
        var savedParent = await Repository.For<DummyModel>().Save(parent);

        var child = new NestedModel { ParentRefId = savedParent.Id };
        await Repository.For<NestedModel>().Save(child);

        // Act
        var splitQueryRepository = Repository.For<DummyModel>().SplitQuery();
        var result = await splitQueryRepository.Get(savedParent.Id);

        // Assert
        result.Should().NotBeNull();
        result.Children.Should().NotBeNull();
    }

    [Fact]
    public async Task Test_CombinedSessions_DelayedAndNoTracking()
    {
        // Arrange
        var model = new DummyModel { Name = "Combined", Children = [] };

        // Act
        var repository = Repository.For<DummyModel>().Delayed().NoTracking();
        var entity = await repository.Save(model);

        // Changes not persisted yet
        (await Repository.For<DummyModel>().GetAll()).Should().BeEmpty();

        await repository.DisposeAsync();

        // Assert - Changes should be persisted after dispose
        var result = await Repository.For<DummyModel>().Get(entity.Id);
        result.Should().NotBeNull();
        result.Name.Should().Be("Combined");
    }

    [Fact]
    public async Task Test_CombinedSessions_LightAndSplitQuery()
    {
        // Arrange
        var model = new DummyModel { Name = "Test", Children = [] };
        await Repository.For<DummyModel>().Save(model);

        // Act
        var repository = Repository.For<DummyModel>().Light().SplitQuery();
        var result = await repository.GetAll();

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Test_DisposeAsync_WithoutChanges()
    {
        // Arrange
        var repository = Repository.For<DummyModel>().Delayed();

        // Act - Dispose without making any changes
        await repository.DisposeAsync();

        // Assert - Should complete without errors
        var count = await Repository.For<DummyModel>().Count();
        count.Should().Be(0);
    }

    [Fact]
    public async Task Test_Dispose_SynchronousWithChanges()
    {
        // Arrange
        var model = new DummyModel { Name = "Sync", Children = [] };
        var repository = Repository.For<DummyModel>().Delayed();
        
        // Act
        await repository.Save(model);
        repository.Dispose();

        // Assert
        var result = await Repository.For<DummyModel>().GetAll();
        result.Should().NotBeEmpty();
        result.First().Name.Should().Be("Sync");
    }

    [Fact]
    public async Task Test_Dispose_WithoutChanges()
    {
        // Arrange
        var repository = Repository.For<DummyModel>().Delayed();

        // Act - Dispose without making changes
        repository.Dispose();

        // Assert - Should complete without errors
        var count = await Repository.For<DummyModel>().Count();
        count.Should().Be(0);
    }

    [Fact]
    public async Task Test_Dispose_NonDelayedSession()
    {
        // Arrange
        var model = new DummyModel { Name = "Immediate", Children = [] };
        var repository = Repository.For<DummyModel>(); // Not delayed

        // Act
        await repository.Save(model);
        repository.Dispose(); // Should do nothing

        // Assert
        var result = await Repository.For<DummyModel>().GetAll();
        result.Should().NotBeEmpty();
    }
}
