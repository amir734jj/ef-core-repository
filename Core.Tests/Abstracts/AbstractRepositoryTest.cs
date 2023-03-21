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

public class AbstractRepositoryTest : IAsyncLifetime
{
    protected readonly IEfRepository Repository;

    protected AbstractRepositoryTest()
    {
        var serviceProvider = new ServiceCollection()
            .Configure<LoggerFilterOptions>(cfg => cfg.MinLevel = LogLevel.None)
            .AddDbContext<EntityDbContext>(x => x
                .UseSqlite("DataSource=file:testdb?mode=memory&cache=shared")
                .EnableSensitiveDataLogging())
            .AddEfRepository<EntityDbContext>(options => options
                .Profile(Assembly.GetExecutingAssembly()))
            .BuildServiceProvider();

        Repository = serviceProvider.GetService<IEfRepository>();
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        var dummyRepository = Repository.For<DummyModel>();
        var nestedRepository = Repository.For<NestedModel>();

        var nestedModels = await nestedRepository.GetAll();
        
        foreach (var nestedModel in nestedModels)
        {
            await nestedRepository.Delete(nestedModel.Id);
        }
        
        var dummyModels = await dummyRepository.GetAll();
        
        foreach (var nestedModel in dummyModels)
        {
            await dummyRepository.Delete(nestedModel.Id);
        }
    }
}