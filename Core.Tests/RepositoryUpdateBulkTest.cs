using System.Linq;
using System.Threading.Tasks;
using Core.Tests.Abstracts;
using Core.Tests.Extensions;
using Core.Tests.Models;
using FluentAssertions;
using Xunit;

namespace Core.Tests;

public class RepositoryUpdateBulkTest : AbstractRepositoryTest
{
    [Fact]
    public async Task Test_BulkUpdate()
    {
        // Arrange
        var model = new DummyModel
        {
            Name = "foo", Children = []
        };

        var entity = await Repository.For<DummyModel>().Save(model);
            
        // Act
        entity.Name = "bar";
        var updatedEntity = (await Repository.For<DummyModel>().BulkUpdate([entity.Id], x => x.Name = "bar")).FirstOrDefault();
            
        // Assert
        updatedEntity.Should()
            .NotBeNull().And
            .BeEquivalentToIgnoreCycles(entity);

        (await Repository.For<DummyModel>().Get(model.Id))
            .Should()
            .BeEquivalentToIgnoreCycles(updatedEntity);
    }
}