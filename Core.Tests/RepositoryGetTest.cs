using System.Linq;
using System.Threading.Tasks;
using Core.Tests.Abstracts;
using Core.Tests.Extensions;
using Core.Tests.Models;
using FluentAssertions;
using Xunit;

namespace Core.Tests;

public class RepositoryGetTest : AbstractRepositoryTest
{
    [Fact]
    public async Task Test_Get_ById()
    {
        // Arrange
        var model = new DummyModel
        {
            Name = "foo", Children = []
        };

        var entity = await Repository.For<DummyModel>().Save(model);

        // Act
        var result = await Repository.For<DummyModel>().Get(entity.Id);

        // Assert
        result.Should()
            .NotBeNull().And
            .BeEquivalentToIgnoreCycles(entity);

        (await Repository.For<DummyModel>().GetAll())
            .Should()
            .ContainSingle().And
            .ContainEquivalentOf(entity, x => x.IgnoringCyclicReferences());
    }

    [Fact]
    public async Task Test_Get_ByExpr()
    {
        // Arrange
        var model = new DummyModel
        {
            Name = "foo", Children = []
        };

        var entity = await Repository.For<DummyModel>().Save(model);

        // Act
        var result = await Repository.For<DummyModel>().Get(x => x.Id == entity.Id);

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(entity);

        (await Repository.For<DummyModel>().GetAll())
            .Should()
            .ContainSingle().And
            .ContainEquivalentOfIgnoreCycles(entity);
    }
    
    [Fact]
    public async Task Test_Get_ByMultipleExpr()
    {
        // Arrange
        var model = new DummyModel
        {
            Name = "foo", Children = []
        };

        var entity = await Repository.For<DummyModel>().Save(model);

        // Act
        var result = await Repository.For<DummyModel>()
            .Get(x => x.Id == entity.Id, x => x.Name == model.Name, x => x.Name != "bar");

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(entity);

        (await Repository.For<DummyModel>().GetAll())
            .Should()
            .ContainSingle().And
            .ContainEquivalentOfIgnoreCycles(entity);
    }

    [Fact]
    public async Task Test_GetAll_ByIds()
    {
        // Arrange
        var model1 = new DummyModel
        {
            Name = "foo", Children = []
        };

        var model2 = new DummyModel
        {
            Name = "bar", Children = []
        };

        var entities = (await Repository.For<DummyModel>().SaveMany(model1, model2)).ToList();

        // Act
        var result = await Repository.For<DummyModel>()
            .GetAll(model1.Id, model2.Id);

        // Assert
        result.Should()
            .NotBeNull().And
            .HaveCount(2).And
            .BeEquivalentToIgnoreCycles(entities);

        (await Repository.For<DummyModel>().GetAll())
            .Should()
            .BeEquivalentToIgnoreCycles(entities);
    }

    [Fact]
    public async Task Test_GetAll_ByExpr()
    {
        // Arrange
        var model1 = new DummyModel
        {
            Name = "foo", Children = []
        };

        var model2 = new DummyModel
        {
            Name = "bar", Children = []
        };

        var entities = (await Repository.For<DummyModel>().SaveMany(model1, model2)).ToList();

        // Act
        var result = await Repository.For<DummyModel>()
            .GetAll(
                filterExprs:
            [
                x => x.Name == model1.Name || x.Name == model2.Name
            ]);

        // Assert
        result.Should()
            .NotBeNull().And
            .HaveCount(2).And
            .BeEquivalentToIgnoreCycles(entities);

        (await Repository.For<DummyModel>().GetAll())
            .Should()
            .HaveCount(2).And
            .BeEquivalentToIgnoreCycles(entities);
    }
    
    [Fact]
    public async Task Test_GetAll_ByMultipleExpr()
    {
        // Arrange
        var model1 = new DummyModel
        {
            Name = "foo", Children = []
        };

        var model2 = new DummyModel
        {
            Name = "bar", Children = []
        };

        var entities = (await Repository.For<DummyModel>().SaveMany(model1, model2)).ToList();

        // Act
        var result = await Repository.For<DummyModel>()
            .GetAll(filterExprs:
            [
                x => x.Name == model1.Name || x.Name == model2.Name, x => x.Name != "baz"
            ]);

        // Assert
        result.Should()
            .NotBeNull().And
            .HaveCount(2).And
            .BeEquivalentToIgnoreCycles(entities);

        (await Repository.For<DummyModel>().GetAll())
            .Should()
            .HaveCount(2).And
            .BeEquivalentToIgnoreCycles(entities);
    }
    
    [Fact]
    public async Task Test_GetAll()
    {
        // Arrange
        var model1 = new DummyModel
        {
            Name = "foo", Children = []
        };

        var model2 = new DummyModel
        {
            Name = "bar", Children = []
        };

        var entities = (await Repository.For<DummyModel>().SaveMany(model1, model2)).ToList();

        // Act
        var result = await Repository.For<DummyModel>().GetAll();

        // Assert
        result.Should()
            .NotBeNull().And
            .HaveCount(2).And
            .BeEquivalentToIgnoreCycles(entities);

        (await Repository.For<DummyModel>().GetAll())
            .Should()
            .HaveCount(2).And
            .BeEquivalentToIgnoreCycles(entities);
    }

    [Fact]
    public async Task Test_GetAllOrderByMaxResult()
    {
        // Arrange
        var model1 = new DummyModel
        {
            Name = "foo", Children = []
        };

        var model2 = new DummyModel
        {
            Name = "bar", Children = []
        };

        await Repository.For<DummyModel>().SaveMany(model1, model2);

        // Act
        var result = await Repository.For<DummyModel>().GetAll(orderBy: x => x.Name, maxResults: 1);

        // Assert
        result.Should()
            .NotBeNull().And
            .HaveCount(1).And
            .BeEquivalentToIgnoreCycles([model2]);

        (await Repository.For<DummyModel>().GetAll())
            .Should()
            .HaveCount(2).And
            .BeEquivalentToIgnoreCycles([model1, model2]);
    }

    [Fact]
    public async Task Test_GetAllOrderByDescMaxResult()
    {
        // Arrange
        var model1 = new DummyModel
        {
            Name = "foo", Children = []
        };

        var model2 = new DummyModel
        {
            Name = "bar", Children = []
        };

        await Repository.For<DummyModel>().SaveMany(model1, model2);

        // Act
        var result = await Repository.For<DummyModel>().GetAll(orderByDesc: x => x.Name, maxResults: 1);

        // Assert
        result.Should()
            .NotBeNull().And
            .HaveCount(1).And
            .BeEquivalentToIgnoreCycles([model1]);

        (await Repository.For<DummyModel>().GetAll())
            .Should()
            .HaveCount(2).And
            .BeEquivalentToIgnoreCycles([model1, model2]);
    }
}