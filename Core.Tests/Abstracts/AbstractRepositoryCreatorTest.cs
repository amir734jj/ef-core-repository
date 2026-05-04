using System.Reflection;
using System.Threading.Tasks;
using Core.Tests.Models;
using EfCoreRepository.Extensions;
using EfCoreRepository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Core.Tests.Abstracts;

public class AbstractRepositoryCreatorTest : IAsyncLifetime
{
    protected readonly ServiceProvider ServiceProvider;

    protected AbstractRepositoryCreatorTest()
    {
        ServiceProvider = new ServiceCollection()
            .Configure<LoggerFilterOptions>(cfg => cfg.MinLevel = LogLevel.None)
            .AddDbContextFactory<EntityDbContext>(x => x
                .UseSqlite("DataSource=file:testdb?mode=memory&cache=shared")
                .EnableSensitiveDataLogging())
            .AddEfRepositoryFactory<EntityDbContext>(options => options
                .Profile(Assembly.GetExecutingAssembly()))
            .BuildServiceProvider();
    }

    protected IEfRepositoryCreator<T> CreatorFor<T>() where T : class, new()
    {
        return ServiceProvider.GetRequiredService<IEfRepositoryCreator<T>>();
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await using var dummyCrud = await CreatorFor<DummyModel>().CreateAsync();
        await using var nestedCrud = await CreatorFor<NestedModel>().CreateAsync();

        var nestedModels = await nestedCrud.GetAll<DummyModel>();

        foreach (var nestedModel in nestedModels)
        {
            await nestedCrud.Delete(nestedModel.Id);
        }

        var dummyModels = await dummyCrud.GetAll<DummyModel>();

        foreach (var dummyModel in dummyModels)
        {
            await dummyCrud.Delete(dummyModel.Id);
        }
    }
}
