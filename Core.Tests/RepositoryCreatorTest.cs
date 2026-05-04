using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Tests.Abstracts;
using Core.Tests.Models;
using EfCoreRepository.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Core.Tests;

public class RepositoryCreatorTest : AbstractRepositoryCreatorTest
{
    [Fact]
    public void Test_Creator_IsRegistered()
    {
        // Act
        var creator = CreatorFor<DummyModel>();

        // Assert
        creator.Should().NotBeNull();
    }

    [Fact]
    public void Test_Creator_IsRegistered_ForAllProfileEntities()
    {
        // Act & Assert
        CreatorFor<DummyModel>().Should().NotBeNull();
        CreatorFor<NestedModel>().Should().NotBeNull();
    }

    [Fact]
    public void Test_Creator_Create_ReturnsBasicCrud()
    {
        // Act
        using var crud = CreatorFor<DummyModel>().Create();

        // Assert
        crud.Should().NotBeNull();
    }

    [Fact]
    public async Task Test_Creator_CreateAsync_ReturnsBasicCrud()
    {
        // Act
        await using var crud = await CreatorFor<DummyModel>().CreateAsync();

        // Assert
        crud.Should().NotBeNull();
    }

    [Fact]
    public async Task Test_Creator_Save_And_Get()
    {
        // Arrange
        var creator = CreatorFor<DummyModel>();

        // Act — save with one session
        int savedId;
        await using (var crud = await creator.CreateAsync())
        {
            var result = await crud.Save(new DummyModel { Name = "creator-test", Children = [] });
            result.Should().NotBeNull();
            savedId = result.Id;
        }

        // Act — read with a separate session
        await using (var crud = await creator.CreateAsync())
        {
            var fetched = await crud.Get(savedId);

            // Assert
            fetched.Should().NotBeNull();
            fetched.Name.Should().Be("creator-test");
        }
    }

    [Fact]
    public async Task Test_Creator_EachCreate_GetsIndependentSession()
    {
        // Act — create two independent sessions
        await using var crud1 = await CreatorFor<DummyModel>().CreateAsync();
        await using var crud2 = await CreatorFor<DummyModel>().CreateAsync();

        // Assert — they should be different instances
        crud1.Should().NotBeSameAs(crud2);
    }

    [Fact]
    public async Task Test_Creator_ParallelQueries()
    {
        // Arrange
        var dummyCreator = CreatorFor<DummyModel>();
        var nestedCreator = CreatorFor<NestedModel>();

        // Seed data
        await using (var crud = await dummyCreator.CreateAsync())
        {
            await crud.Save(new DummyModel { Name = "parallel-1", Children = [] });
            await crud.Save(new DummyModel { Name = "parallel-2", Children = [] });
        }

        // Act — run queries in parallel (the core use case)
        var dummyTask = Task.Run(async () =>
        {
            await using var crud = await dummyCreator.CreateAsync();
            return (await crud.GetAll()).ToList();
        });

        var nestedTask = Task.Run(async () =>
        {
            await using var crud = await nestedCreator.CreateAsync();
            return (await crud.GetAll()).ToList();
        });

        await Task.WhenAll(dummyTask, nestedTask);

        // Assert — parallel execution should not throw
        var dummies = await dummyTask;
        var nesteds = await nestedTask;

        dummies.Should().HaveCountGreaterThanOrEqualTo(2);
        nesteds.Should().NotBeNull();
    }

    [Fact]
    public void Test_StandardDI_StillWorks_WithFactoryRegistration()
    {
        // Act
        using var scope = ServiceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetService<IEfRepository>();
        var directCrud = scope.ServiceProvider.GetService<IBasicCrud<DummyModel>>();

        // Assert
        repo.Should().NotBeNull();
        directCrud.Should().NotBeNull();
    }

    [Fact]
    public async Task Test_Creator_DisposeReleasesContext()
    {
        // Arrange
        var creator = CreatorFor<DummyModel>();

        // Act — create and dispose
        var crud = await creator.CreateAsync();
        await crud.DisposeAsync();

        // Assert — using it after dispose should throw (DbContext is disposed)
        var action = async () => await crud.GetAll();
        await action.Should().ThrowAsync<ObjectDisposedException>();
    }
}
