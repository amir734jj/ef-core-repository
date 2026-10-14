using System;
using System.Linq;
using System.Threading;
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

    [Fact]
    public async Task ExecuteUpdate_UpdatesAllMatchingRows()
    {
        var matching = await Repository.For<DummyModel>().Save(
            new DummyModel { Name = "before", SearchText = "match", Children = [] });
        var nonMatching = await Repository.For<DummyModel>().Save(
            new DummyModel { Name = "before", SearchText = "other", Children = [] });

        var affectedRows = await Repository.For<DummyModel>().ExecuteUpdate(
            [
                entity => entity.SearchText == "match",
                entity => entity.Name == "before"
            ],
            setters => setters.SetProperty(entity => entity.Name, "after"));

        affectedRows.Should().Be(1);
        (await Repository.For<DummyModel>().NoTracking().Get(matching.Id)).Name.Should().Be("after");
        (await Repository.For<DummyModel>().NoTracking().Get(nonMatching.Id)).Name.Should().Be("before");
    }

    [Fact]
    public async Task ExecuteUpdate_ReturnsZeroWhenNoRowsMatch()
    {
        var affectedRows = await Repository.For<DummyModel>().ExecuteUpdate(
            [entity => entity.Id == int.MaxValue],
            setters => setters.SetProperty(entity => entity.Name, "after"));

        affectedRows.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteUpdate_HonorsCancellation()
    {
        var cancellationToken = new CancellationToken(canceled: true);

        Func<Task> act = async () => await Repository.For<DummyModel>().ExecuteUpdate(
            [entity => entity.Id == int.MaxValue],
            setters => setters.SetProperty(entity => entity.Name, "after"),
            cancellationToken);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}