using System;
using System.Reflection;
using Core.Tests.Models;
using EfCoreRepository.Extensions;
using EfCoreRepository.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Core.Tests;

public class RepositoryFactoryTest
{
    [Fact]
    public void Test_Factory_ProfileByAssembly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<EntityDbContext>(x => x.UseSqlite("DataSource=:memory:"));

        // Act
        services.AddEfRepository<EntityDbContext>(options => 
            options.Profile(Assembly.GetExecutingAssembly()));

        var serviceProvider = services.BuildServiceProvider();
        var repository = serviceProvider.GetService<IEfRepository>();

        // Assert
        repository.Should().NotBeNull();
        var dummyRepo = repository.For<DummyModel>();
        dummyRepo.Should().NotBeNull();
    }

    [Fact]
    public void Test_Factory_ProfileByType()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<EntityDbContext>(x => x.UseSqlite("DataSource=:memory:"));

        // Act
        services.AddEfRepository<EntityDbContext>(options => 
            options.Profile<Profiles.DummyModelProfile, DummyModel>());

        var serviceProvider = services.BuildServiceProvider();
        var repository = serviceProvider.GetService<IEfRepository>();

        // Assert
        repository.Should().NotBeNull();
        var dummyRepo = repository.For<DummyModel>();
        dummyRepo.Should().NotBeNull();
    }

    [Fact]
    public void Test_Factory_MultipleProfiles()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<EntityDbContext>(x => x.UseSqlite("DataSource=:memory:"));

        // Act
        services.AddEfRepository<EntityDbContext>(options => 
        {
            options.Profile<Profiles.DummyModelProfile, DummyModel>();
            options.Profile<Profiles.NestedProfile, NestedModel>();
        });

        var serviceProvider = services.BuildServiceProvider();
        var repository = serviceProvider.GetService<IEfRepository>();

        // Assert
        repository.Should().NotBeNull();
        repository.For<DummyModel>().Should().NotBeNull();
        repository.For<NestedModel>().Should().NotBeNull();
    }

    [Fact]
    public void Test_Factory_DependencyInjection_Direct()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<EntityDbContext>(x => x.UseSqlite("DataSource=:memory:"));
        services.AddEfRepository<EntityDbContext>(options => 
            options.Profile(Assembly.GetExecutingAssembly()));

        var serviceProvider = services.BuildServiceProvider();

        // Act - Get IBasicCrud directly from DI
        var dummyRepo = serviceProvider.GetService<IBasicCrud<DummyModel>>();

        // Assert
        dummyRepo.Should().NotBeNull();
    }

    [Fact]
    public void Test_Repository_For_MissingProfile()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<EntityDbContext>(x => x.UseSqlite("DataSource=:memory:"));
        services.AddEfRepository<EntityDbContext>(options => 
            options.Profile<Profiles.DummyModelProfile, DummyModel>());

        var serviceProvider = services.BuildServiceProvider();
        var repository = serviceProvider.GetService<IEfRepository>();

        // Act & Assert - Should throw when profile doesn't exist
        Action action = () => repository.For<NestedModel>();
        action.Should().Throw<Exception>()
            .WithMessage("*Failed to find profile*");
    }
}
