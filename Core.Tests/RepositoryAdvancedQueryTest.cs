using System.Linq;
using System.Threading.Tasks;
using Core.Tests.Abstracts;
using Core.Tests.Models;
using FluentAssertions;
using Xunit;

namespace Core.Tests;

public class RepositoryAdvancedQueryTest : AbstractRepositoryTest
{
    [Fact]
    public async Task Test_GetAll_WithOrderBy()
    {
        // Arrange
        var model1 = new DummyModel { Name = "Charlie", Children = [] };
        var model2 = new DummyModel { Name = "Alpha", Children = [] };
        var model3 = new DummyModel { Name = "Beta", Children = [] };

        await Repository.For<DummyModel>().SaveMany([model1, model2, model3]);

        // Act
        var result = (await Repository.For<DummyModel>().GetAll<DummyModel>(
            orderBy: x => x.Name
        )).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Alpha");
        result[1].Name.Should().Be("Beta");
        result[2].Name.Should().Be("Charlie");
    }

    [Fact]
    public async Task Test_GetAll_WithOrderByDesc()
    {
        // Arrange
        var model1 = new DummyModel { Name = "Charlie", Children = [] };
        var model2 = new DummyModel { Name = "Alpha", Children = [] };
        var model3 = new DummyModel { Name = "Beta", Children = [] };

        await Repository.For<DummyModel>().SaveMany([model1, model2, model3]);

        // Act
        var result = (await Repository.For<DummyModel>().GetAll<DummyModel>(
            orderByDesc: x => x.Name
        )).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Charlie");
        result[1].Name.Should().Be("Beta");
        result[2].Name.Should().Be("Alpha");
    }

    [Fact]
    public async Task Test_GetAll_WithMaxResults()
    {
        // Arrange
        var model1 = new DummyModel { Name = "A", Children = [] };
        var model2 = new DummyModel { Name = "B", Children = [] };
        var model3 = new DummyModel { Name = "C", Children = [] };

        await Repository.For<DummyModel>().SaveMany([model1, model2, model3]);

        // Act
        var result = await Repository.For<DummyModel>().GetAll<DummyModel>(
            maxResults: 2
        );

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Test_GetAll_WithProjection()
    {
        // Arrange
        var model1 = new DummyModel { Name = "Test1", Children = [] };
        var model2 = new DummyModel { Name = "Test2", Children = [] };

        await Repository.For<DummyModel>().SaveMany([model1, model2]);

        // Act
        var result = (await Repository.For<DummyModel>().GetAll(
            project: x => new DummyModel { Id = x.Id, Name = x.Name }
        )).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.All(x => !string.IsNullOrEmpty(x.Name)).Should().BeTrue();
    }

    [Fact]
    public async Task Test_GetAll_WithFilterAndOrderAndMaxResults()
    {
        // Arrange
        var model1 = new DummyModel { Name = "Test1", Children = [] };
        var model2 = new DummyModel { Name = "Test2", Children = [] };
        var model3 = new DummyModel { Name = "Other", Children = [] };

        await Repository.For<DummyModel>().SaveMany([model1, model2, model3]);

        // Act
        var result = (await Repository.For<DummyModel>().GetAll<DummyModel>(
            filterExprs: [x => x.Name.StartsWith("Test")],
            orderByDesc: x => x.Name,
            maxResults: 1
        )).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Test2");
    }

    [Fact]
    public async Task Test_Take()
    {
        // Arrange
        var model1 = new DummyModel { Name = "A", Children = [] };
        var model2 = new DummyModel { Name = "B", Children = [] };
        var model3 = new DummyModel { Name = "C", Children = [] };

        await Repository.For<DummyModel>().SaveMany([model1, model2, model3]);

        // Act
        var result = await Repository.For<DummyModel>().Take(2);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Test_Any_NoParameters()
    {
        // Arrange
        var model = new DummyModel { Name = "Test", Children = [] };
        await Repository.For<DummyModel>().Save(model);

        // Act
        var result = await Repository.For<DummyModel>().Any();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Test_Any_NoParameters_EmptyDatabase()
    {
        // Act
        var result = await Repository.For<DummyModel>().Any();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Test_Count_NoParameters()
    {
        // Arrange
        var model1 = new DummyModel { Name = "A", Children = [] };
        var model2 = new DummyModel { Name = "B", Children = [] };

        await Repository.For<DummyModel>().SaveMany([model1, model2]);

        // Act
        var result = await Repository.For<DummyModel>().Count();

        // Assert
        result.Should().Be(2);
    }
}
