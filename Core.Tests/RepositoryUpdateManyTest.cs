using System.Linq;
using System.Threading.Tasks;
using Core.Tests.Abstracts;
using Core.Tests.Extensions;
using Core.Tests.Models;
using FluentAssertions;
using Xunit;

namespace Core.Tests;

public class RepositoryUpdateManyTest : AbstractRepositoryTest
{
    [Fact]
    public async Task Test_UpdateMany_ById()
    {
        // Arrange
        var model = new DummyModel
        {
            Name = "foo", Children = []
        };

        var entity = await Repository.For<DummyModel>().Save(model);
            
        // Act
        entity.Name = "bar";
        var updatedEntity = (await Repository.For<DummyModel>().UpdateMany([(entity.Id, model)])).FirstOrDefault();
            
        // Assert
        updatedEntity.Should()
            .NotBeNull().And
            .BeEquivalentToIgnoreCycles(entity);

        (await Repository.For<DummyModel>().Get(model.Id))
            .Should()
            .BeEquivalentToIgnoreCycles(updatedEntity);
    }
    
    [Fact]
    public async Task Test_UpdateManyChildren_ById_Add()
    {
        // Arrange
        var model = new DummyModel
        {
            Name = "foo", Children = []
        };

        var parentEntity = await Repository.For<DummyModel>().Save(model);

        var nested = new NestedModel();
        
        var childEntity = await Repository.For<NestedModel>().Save(nested);

        // Act
        parentEntity.Children.Add(childEntity);
        
        var updatedEntity = (await Repository.For<DummyModel>().UpdateMany([(parentEntity.Id, parentEntity)])).FirstOrDefault();

        // Assert
        updatedEntity.Should()
            .NotBeNull().And
            .BeEquivalentToIgnoreCycles(parentEntity);

        (await Repository.For<DummyModel>().Get(model.Id))
            .Should()
            .BeEquivalentToIgnoreCycles(updatedEntity);

        updatedEntity!.Children.Should()
            .HaveCount(1).And
            .ContainEquivalentOfIgnoreCycles(childEntity);
    }
    
    [Fact]
    public async Task Test_UpdateManyChildren_ById_Remove()
    {
        // Arrange
        var model = new DummyModel
        {
            Name = "foo", Children = []
        };

        var parentEntity = await Repository.For<DummyModel>().Save(model);

        var nested = new NestedModel
        {
            ParentRefId = parentEntity.Id
        };
        
        var childEntity = await Repository.For<NestedModel>().Save(nested);

        // Act
        parentEntity.Children.Remove(childEntity);
        
        var updatedEntity = (await Repository.For<DummyModel>().UpdateMany([(parentEntity.Id, parentEntity)])).FirstOrDefault();

        // Assert
        updatedEntity.Should()
            .NotBeNull().And
            .BeEquivalentToIgnoreCycles(parentEntity);

        (await Repository.For<DummyModel>().Get(model.Id))
            .Should()
            .BeEquivalentToIgnoreCycles(updatedEntity);

        updatedEntity!.Children.Should()
            .BeEmpty();
    }
    
    [Fact]
    public async Task Test_UpdateMany_Action()
    {
        // Arrange
        var model = new DummyModel
        {
            Name = "foo", Children = []
        };

        var entity = await Repository.For<DummyModel>().Save(model);
            
        // Act
        var updatedEntity = (await Repository.For<DummyModel>().UpdateMany([(entity.Id, x =>
        {
            x.Name = "bar";
        })])).FirstOrDefault();
            
        // Assert
        updatedEntity.Should()
            .NotBeNull().And
            .BeEquivalentToIgnoreCycles(entity);

        (await Repository.For<DummyModel>().Get(model.Id))
            .Should()
            .BeEquivalentToIgnoreCycles(updatedEntity);
    }
}