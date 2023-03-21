using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Tests.Abstracts;
using Core.Tests.Extensions;
using Core.Tests.Models;
using FluentAssertions;
using Xunit;

namespace Core.Tests;

public class RepositorySessionTest : AbstractRepositoryTest
{
    [Fact]
    public async Task Test_DelayedSession()
    {
        // Arrange
        var model = new DummyModel
        {
            Name = "foo", Children = new List<NestedModel>()
        };

        var delayedRepository = Repository.For<DummyModel>().Delayed();

        var entity = await delayedRepository.Save(model);
        
        // Act
        (await delayedRepository.GetAll())
            .Should()
            .BeEmpty();

        await delayedRepository.DisposeAsync();

        // Assert
        (await delayedRepository.GetAll())
            .Should()
            .NotBeEmpty().And
            .ContainEquivalentOfIgnoreCycles(entity);
    }
}