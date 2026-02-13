using System.Linq;
using System.Threading.Tasks;
using Core.Tests.Abstracts;
using Core.Tests.Models;
using FluentAssertions;
using Xunit;

namespace Core.Tests;

public class RepositoryHasReferencesTest : AbstractRepositoryTest
{
    [Fact]
    public async Task Test_HasReferences_NoReferences_ReturnsFalse()
    {
        // Arrange
        var model = new DummyModel
        {
            Name = "Parent without children",
            Children = []
        };

        var entity = await Repository.For<DummyModel>().Save(model);

        // Act
        var hasReferences = await Repository.For<DummyModel>().HasReferences(entity);

        // Assert
        hasReferences.Should().BeFalse();
    }

    [Fact]
    public async Task Test_HasReferences_WithCollectionReferences_ReturnsTrue()
    {
        // Arrange
        var parent = new DummyModel
        {
            Name = "Parent with children",
            Children = []
        };

        var savedParent = await Repository.For<DummyModel>().Save(parent);

        var child = new NestedModel
        {
            ParentRefId = savedParent.Id
        };

        await Repository.For<NestedModel>().Save(child);

        // Act - Reload parent to ensure navigation is tracked
        var parentEntity = await Repository.For<DummyModel>().Get(savedParent.Id);
        var hasReferences = await Repository.For<DummyModel>().HasReferences(parentEntity);

        // Assert
        hasReferences.Should().BeTrue();
    }

    [Fact]
    public async Task Test_HasReferences_WithMultipleCollectionReferences_ReturnsTrue()
    {
        // Arrange
        var parent = new DummyModel
        {
            Name = "Parent with multiple children",
            Children = []
        };

        var savedParent = await Repository.For<DummyModel>().Save(parent);

        var child1 = new NestedModel
        {
            ParentRefId = savedParent.Id
        };
        var child2 = new NestedModel
        {
            ParentRefId = savedParent.Id
        };

        await Repository.For<NestedModel>().SaveMany([child1, child2]);

        // Act
        var parentEntity = await Repository.For<DummyModel>().Get(savedParent.Id);
        var hasReferences = await Repository.For<DummyModel>().HasReferences(parentEntity);

        // Assert
        hasReferences.Should().BeTrue();
    }

    [Fact]
    public async Task Test_HasReferences_ChildEntity_NoReferences_ReturnsFalse()
    {
        // Arrange
        var parent = new DummyModel
        {
            Name = "Parent",
            Children = []
        };

        var savedParent = await Repository.For<DummyModel>().Save(parent);

        var child = new NestedModel
        {
            ParentRefId = savedParent.Id
        };

        var savedChild = await Repository.For<NestedModel>().Save(child);

        // Act - Check if child has references (it shouldn't, as it's a dependent entity)
        var childEntity = await Repository.For<NestedModel>().Get(savedChild.Id);
        var hasReferences = await Repository.For<NestedModel>().HasReferences(childEntity);

        // Assert
        hasReferences.Should().BeFalse();
    }

    [Fact]
    public async Task Test_HasReferences_AfterDeletingChildren_ReturnsFalse()
    {
        // Arrange
        var parent = new DummyModel
        {
            Name = "Parent with children",
            Children = []
        };

        var savedParent = await Repository.For<DummyModel>().Save(parent);

        var child = new NestedModel
        {
            ParentRefId = savedParent.Id
        };

        var savedChild = await Repository.For<NestedModel>().Save(child);

        // Act - Delete the child and check if parent still has references
        await Repository.For<NestedModel>().Delete(savedChild.Id);
        
        var parentEntity = await Repository.For<DummyModel>().Get(savedParent.Id);
        var hasReferences = await Repository.For<DummyModel>().HasReferences(parentEntity);

        // Assert
        hasReferences.Should().BeFalse();
    }
}
