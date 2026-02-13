using System;
using System.Collections.Generic;
using System.Linq;
using Core.Tests.Models;
using EfCoreRepository;
using FluentAssertions;
using Xunit;

namespace Core.Tests;

public class EntityProfileTest
{
    private class TestMapProfile : EntityProfile<DummyModel>
    {
        public TestMapProfile()
        {
            Map(x => x.Name);
            Map(x => x.Id);
        }

        protected override void Update(DummyModel entity, DummyModel dto)
        {
            entity.Name = dto.Name;
        }
    }

    private class TestMapAllProfile : EntityProfile<DummyModel>
    {
        public TestMapAllProfile()
        {
            MapAll(x => x.Children); // Ignore Children property
        }
    }

    private class TestMapAllNoIgnoreProfile : EntityProfile<DummyModel>
    {
        public TestMapAllNoIgnoreProfile()
        {
            MapAll(); // Map all properties
        }
    }

    [Fact]
    public void Test_EntityProfile_Map()
    {
        // Arrange
        var profile = new TestMapProfile();
        var entityTypes = new List<Type> { typeof(DummyModel) };

        // Act
        var mapping = profile.ToEntityMapping(entityTypes);

        // Assert
        mapping.Should().NotBeNull();
    }

    [Fact]
    public void Test_EntityProfile_MapAll_WithIgnored()
    {
        // Arrange
        var profile = new TestMapAllProfile();
        var entityTypes = new List<Type> { typeof(DummyModel) };

        // Act
        var mapping = profile.ToEntityMapping(entityTypes);

        // Assert
        mapping.Should().NotBeNull();
    }

    [Fact]
    public void Test_EntityProfile_MapAll_NoIgnore()
    {
        // Arrange
        var profile = new TestMapAllNoIgnoreProfile();
        var entityTypes = new List<Type> { typeof(DummyModel) };

        // Act
        var mapping = profile.ToEntityMapping(entityTypes);

        // Assert
        mapping.Should().NotBeNull();
    }

    [Fact]
    public void Test_EntityProfile_Update()
    {
        // Arrange
        var profile = new TestMapProfile();
        var entity = new DummyModel { Name = "Original", Children = [] };
        var dto = new DummyModel { Name = "Updated", Children = [] };

        // Act - The Update method is called through the mapping
        var entityTypes = new List<Type> { typeof(DummyModel) };
        var mapping = profile.ToEntityMapping(entityTypes);
        mapping.Update(entity, dto);

        // Assert
        entity.Name.Should().Be("Updated");
    }

    [Fact]
    public void Test_EntityProfile_Include()
    {
        // Arrange
        var profile = new TestMapProfile();
        var entityTypes = new List<Type> { typeof(DummyModel) };
        var mapping = profile.ToEntityMapping(entityTypes);

        var queryable = new List<DummyModel>
        {
            new() { Name = "Test", Children = [] }
        }.AsQueryable();

        // Act
        var result = mapping.Include(queryable);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IQueryable<DummyModel>>();
    }
}
