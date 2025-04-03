using System.Linq;
using System.Threading.Tasks;
using Core.Tests.Abstracts;
using Core.Tests.Models;
using EfCoreRepository.Extensions;
using FluentAssertions;
using Xunit;

namespace Core.Tests;

public class RepositoryDeleteTest : AbstractRepositoryTest
{
    [Fact]
    public async Task Test_Delete_ById()
    {
        // Arrange
        var model = new DummyModel
        {
            Name = "foo", Children = []
        };

        var entity = await Repository.For<DummyModel>().Save(model);

        // Act
        var result = await Repository.For<DummyModel>().Delete(entity.Id);

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(entity);
        

        (await Repository.For<DummyModel>().GetAll())
            .Should()
            .BeEmpty();
    }

    [Fact]
    public async Task Test_Delete_ByExpr()
    {
        // Arrange
        var model = new DummyModel
        {
            Name = "foo", Children = []
        };

        var entity = await Repository.For<DummyModel>().Save(model);

        // Act
        var result = await Repository.For<DummyModel>().Delete(x => x.Id == entity.Id);

        // Assert
        result.Should()
            .NotBeNull().And
            .Be(entity);

        (await Repository.For<DummyModel>().GetAll())
            .Should()
            .BeEmpty();
    }

    [Fact]
    public async Task Test_DeleteMany_ByIds()
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
            .DeleteMany(model1.Id, model2.Id);

        // Assert
        result.Should()
            .NotBeNull().And
            .HaveCount(2).And
            .BeEquivalentTo(entities);

        (await Repository.For<DummyModel>().GetAll())
            .Should()
            .BeEmpty();
    }

    [Fact]
    public async Task Test_DeleteMany_ByExpr()
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
            .DeleteMany(x => x.Name == model1.Name || x.Name == model2.Name);

        // Assert
        result.Should()
            .NotBeNull().And
            .HaveCount(2).And
            .BeEquivalentTo(entities);

        (await Repository.For<DummyModel>().GetAll())
            .Should()
            .BeEmpty();
    }
}