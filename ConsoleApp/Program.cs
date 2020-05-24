using System.Reflection;
using System.Threading.Tasks;
using ConsoleApp.Extensions;
using ConsoleApp.Models;
using Core.Extensions;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging(cfg => cfg.AddConsole())
                .Configure<LoggerFilterOptions>(cfg => cfg.MinLevel = LogLevel.Trace)
                .AddDbContext<EntityDbContext>(x => x.UseInMemoryDatabase("test"))
                .AddEfRepository<EntityDbContext>(options => options
                    .Profiles(Assembly.GetExecutingAssembly()))
                .BuildServiceProvider();

            var repo = serviceProvider.GetService<IEfRepository>().For<DummyModel, int>();

            var entity = await repo.Save(new DummyModel {Name = "Foo"});
            var dto = (await repo.Get(1)).DeepClone();
            dto.Name = "Bar";
            await repo.Update(entity.Id, dto);
            
            (await repo.Get(1)).Name.ShouldBe("Bar");
        }
    }
}