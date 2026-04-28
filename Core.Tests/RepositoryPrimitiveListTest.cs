using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Tests.Abstracts;
using Core.Tests.Models;
using FluentAssertions;
using Xunit;

namespace Core.Tests;

public class RepositoryPrimitiveListTest : AbstractRepositoryTest
{
    [Fact]
    public async Task Test_Save_WithStringList()
    {
        // Arrange
        var model = new TaggedModel
        {
            Title = "Test Item",
            Tags = ["alpha", "beta", "gamma"]
        };

        // Act
        var result = await Repository.For<TaggedModel>().Save(model);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Item");
        result.Tags.Should().BeEquivalentTo(["alpha", "beta", "gamma"]);
    }

    [Fact]
    public async Task Test_Save_WithEmptyStringList()
    {
        // Arrange
        var model = new TaggedModel
        {
            Title = "Empty Tags",
            Tags = []
        };

        // Act
        var result = await Repository.For<TaggedModel>().Save(model);

        // Assert
        result.Should().NotBeNull();
        result.Tags.Should().BeEmpty();
    }

    [Fact]
    public async Task Test_Update_StringList()
    {
        // Arrange
        var dal = Repository.For<TaggedModel>();
        var model = await dal.Save(new TaggedModel
        {
            Title = "Original",
            Tags = ["one", "two"]
        });

        // Act
        await dal.Update(model.Id, x =>
        {
            x.Tags = ["three", "four", "five"];
        });

        // Assert
        var updated = (await dal.GetAll<TaggedModel>(filterExprs: [x => x.Id == model.Id])).First();
        updated.Tags.Should().BeEquivalentTo(["three", "four", "five"]);
    }

    [Fact]
    public async Task Test_Update_ClearStringList()
    {
        // Arrange
        var dal = Repository.For<TaggedModel>();
        var model = await dal.Save(new TaggedModel
        {
            Title = "Has Tags",
            Tags = ["a", "b"]
        });

        // Act
        await dal.Update(model.Id, x =>
        {
            x.Tags = [];
        });

        // Assert
        var updated = (await dal.GetAll<TaggedModel>(filterExprs: [x => x.Id == model.Id])).First();
        updated.Tags.Should().BeEmpty();
    }

    [Fact]
    public async Task Test_MapAll_DoesNotCrash_WithStringList()
    {
        // This test verifies that MapAll() in the profile doesn't throw
        // "Missing KEY attribute on the class declaration for nested entity: String"
        var dal = Repository.For<TaggedModel>();

        // Act & Assert - should not throw
        var result = await dal.Save(new TaggedModel
        {
            Title = "MapAll Test",
            Tags = ["x", "y"]
        });

        result.Should().NotBeNull();
        result.Tags.Should().HaveCount(2);
    }
}
