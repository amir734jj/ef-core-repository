using System.Linq;
using System.Threading.Tasks;
using Core.Tests.Abstracts;
using Core.Tests.Extensions;
using Core.Tests.Models;
using FluentAssertions;
using Xunit;

namespace Core.Tests;

public class RepositorySaveTest : AbstractRepositoryTest
{
    [Fact]
    public async Task Test_Save()
    {
        // Arrange
        var model = new DummyModel
        {
            Name = "foo", Children = []
        };

        // Act
        var result = await Repository.For<DummyModel>().Save(model);

        // Assert
        result.Should()
            .NotBeNull().And
            .BeEquivalentToIgnoreCycles(model).And
            .BeEquivalentToIgnoreCycles(await Repository.For<DummyModel>().Get(model.Id));

        result.Children.Should().BeEmpty();
    }

    [Fact]
    public async Task Test_SaveMany()
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

        // Act
        var result = (await Repository.For<DummyModel>().SaveMany([model1, model2])).ToList();

        // Assert
        result.Should()
            .NotBeNull().And
            .HaveCount(2).And
            .OnlyContain(x => x.Name == "foo" || x.Name == "bar");

        var model1Result = result.First(x => x.Name == model1.Name);
        var model2Result = result.First(x => x.Name == model2.Name);

        model1Result.Should()
            .NotBeNull().And
            .BeEquivalentToIgnoreCycles(model1).And
            .BeEquivalentToIgnoreCycles(await Repository.For<DummyModel>().Get([x => x.Name == model1.Name]));

        model2Result.Should()
            .NotBeNull().And
            .BeEquivalentToIgnoreCycles(model2).And
            .BeEquivalentToIgnoreCycles(await Repository.For<DummyModel>().Get([x => x.Name == model2.Name]));
    }
}