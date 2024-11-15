using System.Threading.Tasks;
using Core.Tests.Abstracts;
using Core.Tests.Extensions;
using Core.Tests.Models;
using FluentAssertions;
using Xunit;

namespace Core.Tests;

public class RepositoryUpdateTest : AbstractRepositoryTest
{
    [Fact]
    public async Task Test_Update_ById()
    {
        // Arrange
        var model = new DummyModel
        {
            Name = "foo", Children = []
        };

        var entity = await Repository.For<DummyModel>().Save(model);
            
        // Act
        entity.Name = "bar";
        var updatedEntity = await Repository.For<DummyModel>().Update(entity.Id, model);
            
        // Assert
        updatedEntity.Should()
            .NotBeNull().And
            .BeEquivalentToIgnoreCycles(entity);

        (await Repository.For<DummyModel>().Get(model.Id))
            .Should()
            .BeEquivalentToIgnoreCycles(updatedEntity);
    }
    
    [Fact]
    public async Task Test_UpdateChildren_ById_Add()
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
        
        var updatedEntity = await Repository.For<DummyModel>().Update(parentEntity.Id, parentEntity);

        // Assert
        updatedEntity.Should()
            .NotBeNull().And
            .BeEquivalentToIgnoreCycles(parentEntity);

        (await Repository.For<DummyModel>().Get(model.Id))
            .Should()
            .BeEquivalentToIgnoreCycles(updatedEntity);

        updatedEntity.Children.Should()
            .HaveCount(1).And
            .ContainEquivalentOfIgnoreCycles(childEntity);
    }
    
    [Fact]
    public async Task Test_UpdateChildren_ById_Remove()
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
        
        var updatedEntity = await Repository.For<DummyModel>().Update(parentEntity.Id, parentEntity);

        // Assert
        updatedEntity.Should()
            .NotBeNull().And
            .BeEquivalentToIgnoreCycles(parentEntity);

        (await Repository.For<DummyModel>().Get(model.Id))
            .Should()
            .BeEquivalentToIgnoreCycles(updatedEntity);

        updatedEntity.Children.Should()
            .BeEmpty();
    }
    
    [Fact]
    public async Task Test_Update_Action()
    {
        // Arrange
        var model = new DummyModel
        {
            Name = "foo", Children = []
        };

        var entity = await Repository.For<DummyModel>().Save(model);
            
        // Act
        var updatedEntity = await Repository.For<DummyModel>().Update(entity.Id, x =>
        {
            x.Name = "bar";
        });
            
        // Assert
        updatedEntity.Should()
            .NotBeNull().And
            .BeEquivalentToIgnoreCycles(entity);

        (await Repository.For<DummyModel>().Get(model.Id))
            .Should()
            .BeEquivalentToIgnoreCycles(updatedEntity);
    }
        
    [Fact]
    public async Task Test_Update_ByExpr()
    {
        // Arrange
        var model = new DummyModel
        {
            Name = "foo", Children = []
        };

        var entity = await Repository.For<DummyModel>().Save(model);
            
        // Act
        entity.Name = "bar";
        var updatedEntity = await Repository.For<DummyModel>().Update(entity, x => x.Id == entity.Id);

        // Assert
        updatedEntity.Should()
            .NotBeNull().And
            .BeEquivalentToIgnoreCycles(entity);

        (await Repository.For<DummyModel>().Get(model.Id))
            .Should()
            .BeEquivalentToIgnoreCycles(updatedEntity);
    }
    
    [Fact]
    public async Task Test_UpdateChildren_ByExpr_Add()
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
        
        var updatedEntity = await Repository.For<DummyModel>().Update(parentEntity, x => x.Id == parentEntity.Id);

        // Assert
        updatedEntity.Should()
            .NotBeNull().And
            .BeEquivalentToIgnoreCycles(parentEntity);

        (await Repository.For<DummyModel>().Get(model.Id))
            .Should()
            .BeEquivalentToIgnoreCycles(updatedEntity);

        updatedEntity.Children.Should()
            .HaveCount(1).And
            .ContainEquivalentOfIgnoreCycles(childEntity);
    }
    
    [Fact]
    public async Task Test_UpdateChildren_ByExpr_Remove()
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
        
        var updatedEntity = await Repository.For<DummyModel>().Update(parentEntity, x => x.Id == parentEntity.Id);

        // Assert
        updatedEntity.Should()
            .NotBeNull().And
            .BeEquivalentToIgnoreCycles(parentEntity);

        (await Repository.For<DummyModel>().Get(model.Id))
            .Should()
            .BeEquivalentToIgnoreCycles(updatedEntity);

        updatedEntity.Children.Should()
            .BeEmpty();
    }
}