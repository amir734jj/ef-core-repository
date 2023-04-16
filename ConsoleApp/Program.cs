using System;
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
using Newtonsoft.Json;
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
                .Configure<JsonSerializerSettings>(x => x.ReferenceLoopHandling = ReferenceLoopHandling.Ignore)
                .AddDbContext<EntityDbContext>(x => x.UseInMemoryDatabase("test"))
                .AddEfRepository<EntityDbContext>(options => options
                    .Profile(Assembly.Load("Core.Tests")))
                .BuildServiceProvider();

            var dal = serviceProvider.GetService<IBasicCrud<DummyModel>>();
            var entities = await dal.Save(new DummyModel {Name = "foo", Children = new List<NestedModel> { new NestedModel()}});
            var dto = (await dal.Get(1)).DeepClone();
            dto.Name = "bar";
            await dal.Update(entities.Id, dto);

            var updatedEntity = await dal.Get(1);
            
            updatedEntity.Name.ShouldBe("bar");
            updatedEntity.Children.ShouldNotBeNull();

            var children = await serviceProvider.GetService<IBasicCrud<NestedModel>>().GetAll();
            children.Count().ShouldNotBe(0);
        }
    }
}