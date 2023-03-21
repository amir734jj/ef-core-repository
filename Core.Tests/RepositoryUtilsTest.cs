using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Tests.Abstracts;
using Core.Tests.Models;
using FluentAssertions;
using Xunit;

namespace Core.Tests;

public class RepositoryUtilsTest : AbstractRepositoryTest
{
    [Fact]
    public async Task Test_Count_ByExpr()
    {
        // Arrange
        var model = new DummyModel
        {
            Name = "foo", Children = new List<NestedModel>()
        };

        var entity = await Repository.For<DummyModel>().Save(model);

        // Act
        var result = await Repository.For<DummyModel>().Count(x => x.Name == entity.Name);

        // Assert
        result.Should()
            .Be(1);

        (await Repository.For<DummyModel>().Count())
            .Should()
            .Be(1);
    }
    
    [Fact]
    public async Task Test_Count_ByMultipleExpr()
    {
        // Arrange
        var model = new DummyModel
        {
            Name = "foo", Children = new List<NestedModel>()
        };

        var entity = await Repository.For<DummyModel>().Save(model);

        // Act
        var result = await Repository.For<DummyModel>().Count(x => x.Name == entity.Name, x => x.Name != "bar");

        // Assert
        result.Should()
            .Be(1);

        (await Repository.For<DummyModel>().Count())
            .Should()
            .Be(1);
    }

    [Fact]
    public async Task Test_Any_ByExpr()
    {
        // Arrange
        var model = new DummyModel
        {
            Name = "foo", Children = new List<NestedModel>()
        };

        var entity = await Repository.For<DummyModel>().Save(model);

        // Act
        var result = await Repository.For<DummyModel>().Any(x => x.Name == entity.Name);

        // Assert
        result.Should()
            .Be(true);

        (await Repository.For<DummyModel>().Count(x => x.Name == entity.Name))
            .Should()
            .Be(1);
    }

    [Fact]
    public async Task Test_Any_ByMultipleExpr()
    {
        // Arrange
        var model = new DummyModel
        {
            Name = "foo", Children = new List<NestedModel>()
        };

        var entity = await Repository.For<DummyModel>().Save(model);

        // Act
        var result = await Repository.For<DummyModel>().Any(x => x.Name == entity.Name, x => x.Name != "bar");

        // Assert
        result.Should()
            .Be(true);

        (await Repository.For<DummyModel>().Count(x => x.Name == entity.Name))
            .Should()
            .Be(1);
    }
}