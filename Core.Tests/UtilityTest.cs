using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Tests.Abstracts;
using Core.Tests.Models;
using Core.Tests.Profiles;
using FluentAssertions;
using Xunit;

namespace Core.Tests;

public class UtilityTest : AbstractRepositoryTest
{
    [Fact]
    public async Task Test_FilterExpression_EmptyArray()
    {
        // Arrange & Act - Using repository's GetAll with empty IDs should return all items
        var model1 = new DummyModel { Name = "Test1", Children = [] };
        var model2 = new DummyModel { Name = "Test2", Children = [] };
        await Repository.For<DummyModel>().SaveMany([model1, model2]);

        var result = await Repository.For<DummyModel>().GetAll<int>([]);

        // Assert - Empty array should match everything
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Test_FilterExpression_SingleId()
    {
        // Arrange
        var model = new DummyModel { Name = "Test", Children = [] };
        var saved = await Repository.For<DummyModel>().Save(model);

        // Act
        var result = await Repository.For<DummyModel>().Get(saved.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(saved.Id);
    }

    [Fact]
    public async Task Test_FilterExpression_MultipleIds()
    {
        // Arrange
        var model1 = new DummyModel { Name = "Test1", Children = [] };
        var model2 = new DummyModel { Name = "Test2", Children = [] };
        var model3 = new DummyModel { Name = "Test3", Children = [] };
        
        await Repository.For<DummyModel>().SaveMany([model1, model2, model3]);

        // Act - Get multiple by IDs
        var result = (await Repository.For<DummyModel>().GetAll([model1.Id, model2.Id])).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(x => x.Id == model1.Id);
        result.Should().Contain(x => x.Id == model2.Id);
    }

    [Fact]
    public void Test_ModifyList_NullParameters()
    {
        // Arrange
        var testProfile = new Profiles.DummyModelProfile();
        IList<NestedModel> entityList = null;
        IList<NestedModel> dtoList = null;

        // Act - ModifyList should handle null lists
        DummyModelProfile.TestModifyList(entityList!, dtoList, x => x.Id);

        // Assert - Should not throw
        entityList.Should().BeNull();
    }

    [Fact]
    public void Test_ModifyList_Additions()
    {
        // Arrange
        var testProfile = new Profiles.DummyModelProfile();
        var entityList = new List<NestedModel>
        {
            new() { Id = 1, ParentRefId = 1 }
        };
        var dtoList = new List<NestedModel>
        {
            new() { Id = 1, ParentRefId = 1 },
            new() { Id = 2, ParentRefId = 1 }
        };

        // Act
        DummyModelProfile.TestModifyList(entityList, dtoList, x => x.Id);

        // Assert - Should add the new item
        entityList.Should().HaveCount(2);
        entityList.Should().Contain(x => x.Id == 2);
    }

    [Fact]
    public void Test_ModifyList_Deletions()
    {
        // Arrange
        var testProfile = new Profiles.DummyModelProfile();
        var entityList = new List<NestedModel>
        {
            new() { Id = 1, ParentRefId = 1 },
            new() { Id = 2, ParentRefId = 1 }
        };
        var dtoList = new List<NestedModel>
        {
            new() { Id = 1, ParentRefId = 1 }
        };

        // Act
        DummyModelProfile.TestModifyList(entityList, dtoList, x => x.Id);

        // Assert - Should remove the item not in dto
        entityList.Should().HaveCount(1);
        entityList.Should().NotContain(x => x.Id == 2);
    }
}
