using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Core.Tests.Models;
using EfCoreRepository;
using EfCoreRepository.Interfaces;
using EfCoreRepository.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace Core.Tests;

/// <summary>
/// Guards the two design promises of the library: every consumer-facing operation is on a
/// mockable interface, and the public surface that existed before the read-only/join refactor
/// is still intact (nothing was dropped).
/// </summary>
public class MockabilityTest
{
    [Fact]
    public async Task IBasicCrud_ReadAndWrite_AreMockable()
    {
        var dal = new Mock<IBasicCrud<DummyModel>>();

        dal.Setup(x => x.Count()).ReturnsAsync(42);
        dal.Setup(x => x.Save(It.IsAny<DummyModel>())).ReturnsAsync((DummyModel d) => d);
        dal.Setup(x => x.GetAll<DummyModel>(
                It.IsAny<Expression<Func<DummyModel, bool>>[]>(),
                It.IsAny<Func<IQueryable<DummyModel>, IQueryable<DummyModel>>>(),
                It.IsAny<Ordering<DummyModel>>(),
                It.IsAny<Expression<Func<DummyModel, DummyModel>>>(),
                It.IsAny<int?>(),
                It.IsAny<Expression<Func<DummyModel, object>>>()))
            .ReturnsAsync([new DummyModel { Name = "Mocked" }]);

        var crud = dal.Object;

        (await crud.Count()).Should().Be(42);
        (await crud.Save(new DummyModel { Name = "X" })).Name.Should().Be("X");
        (await crud.GetAll<DummyModel>()).Single().Name.Should().Be("Mocked");
    }

    [Fact]
    public void Join_ReturnsAMockableReadOnlyCrud()
    {
        var joinResult = new Mock<IReadOnlyCrud<Joined<DummyModel, NestedModel>>>().Object;

        var dal = new Mock<IBasicCrud<DummyModel>>();
        dal.Setup(x => x.Join(
                It.IsAny<Expression<Func<DummyModel, int?>>>(),
                It.IsAny<Expression<Func<NestedModel, int?>>>(),
                It.IsAny<JoinType>()))
            .Returns(joinResult);

        var result = dal.Object.Join<NestedModel, int?>(d => d.Id, n => n.ParentRefId);

        result.Should().BeSameAs(joinResult);
    }

    [Fact]
    public async Task ReadOnlyCrud_IsIndependentlyMockable()
    {
        // A service that only needs to read can depend on the narrower read-only surface.
        var reader = new Mock<IReadOnlyCrud<DummyModel>>();
        reader.Setup(x => x.Any()).ReturnsAsync(true);

        (await reader.Object.Any()).Should().BeTrue();
    }

    [Fact]
    public void IBasicCrud_StillExposesEveryMethodFromBeforeTheRefactor()
    {
        var members = AllMethodNames(typeof(IBasicCrud<DummyModel>));

        // The complete pre-refactor surface (Singles, Many, Utils, Bulk, UnSafe) plus the
        // session toggles and IDisposable/IAsyncDisposable, regardless of which interface now hosts them.
        var expected = new[]
        {
            "Get", "GetAll", "Update", "Delete", "Save", "SaveMany", "DeleteMany",
            "BulkUpdate", "Count", "Any", "Take", "HasReferences",
            "Delayed", "Light", "NoTracking", "SplitQuery", "Dispose", "DisposeAsync",
        };

        members.Should().Contain(expected);
    }

    [Theory]
    [InlineData("Get", 2)]        // Get<TId>(id) and Get(filterExprs)
    [InlineData("GetAll", 3)]     // GetAll<TProject>(...), GetAll<TId>(ids), GetAll()
    [InlineData("Update", 2)]     // Update<TId>(id, dto) and Update<TId>(id, updater)
    [InlineData("Delete", 2)]     // Delete<TId>(id) and Delete(filterExprs)
    [InlineData("DeleteMany", 2)] // DeleteMany<TId>(ids) and DeleteMany(filterExprs)
    [InlineData("Count", 2)]      // Count() and Count(filterExprs)
    [InlineData("Any", 2)]        // Any() and Any(filterExprs)
    public void IBasicCrud_KeepsAllOverloads(string method, int expectedOverloads)
    {
        AllMethods(typeof(IBasicCrud<DummyModel>))
            .Count(m => m.Name == method)
            .Should().BeGreaterThanOrEqualTo(expectedOverloads);
    }

    private static IEnumerable<MethodInfo> AllMethods(Type t)
        => t.GetInterfaces().Append(t).SelectMany(i => i.GetMethods());

    private static HashSet<string> AllMethodNames(Type t)
        => AllMethods(t).Select(m => m.Name).ToHashSet();
}
