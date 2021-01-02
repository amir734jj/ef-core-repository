using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ConsoleApp.Extensions;
using Core.Tests;
using Core.Tests.Models;
using EfCoreRepository.Extensions;
using EfCoreRepository.Interfaces;
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
                    .Profiles(Assembly.Load("Core.Tests")))
                .BuildServiceProvider();

            var repository = serviceProvider.GetService<IEfRepository>();
            var dal = repository.For<DummyModel>();
            var entity = await dal.Save(new DummyModel {Name = "Foo", Children = new List<Nested> { new Nested()}});
            var dto = (await dal.Get(1)).DeepClone();
            dto.Name = "Bar";
            await dal.Update(entity.Id, dto);

            var updatedEntity = await dal.Get(1);
            
            updatedEntity.Name.ShouldBe("Bar");
            updatedEntity.Children.ShouldNotBeNull();

            var children = await repository.For<Nested>().GetAll();
            children.Count().ShouldNotBe(0);
        }
    }
}