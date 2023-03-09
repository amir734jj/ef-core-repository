using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Core.Tests.Models;
using EfCoreRepository.Extensions;
using EfCoreRepository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Xunit;

namespace Core.Tests
{
    public class RepositoryTest : IAsyncLifetime
    {
        private readonly IEfRepository _repository;

        public RepositoryTest()
        {
            var serviceProvider = new ServiceCollection()
                .Configure<LoggerFilterOptions>(cfg => cfg.MinLevel = LogLevel.None)
                .AddDbContext<EntityDbContext>(x => x
                    .UseInMemoryDatabase("database"))
                .AddEfRepository<EntityDbContext>(options => options
                    .Profile(Assembly.GetExecutingAssembly()))
                .BuildServiceProvider();

            _repository = serviceProvider.GetService<IEfRepository>();
        }
        
        [Fact]
        public async Task Test__Save()
        {
            // Arrange
            var model = new DummyModel
            {
                Name = "Foo", Children = new List<Nested>
                {
                    new Nested()
                }
            };

            // Act
            var result = (await _repository.For<DummyModel>().Save(model)).ToList();
            
            // Assert
            Assert.NotEmpty(result);
            AssertJsonEquals(model, result.First());
            AssertJsonEquals(model, await _repository.For<DummyModel>().Get(model.Id));
        }
        
        [Fact]
        public async Task Test__SaveMany()
        {
            // Arrange
            var model1 = new DummyModel
            {
                Name = "Foo", Children = new List<Nested>
                {
                    new Nested()
                }
            };
            
            var model2 = new DummyModel
            {
                Name = "Foo", Children = new List<Nested>
                {
                    new Nested()
                }
            };

            // Act
            var result = (await _repository.For<DummyModel>().Save(model1, model2)).ToList();
            
            // Assert
            AssertJsonEquals(new List<DummyModel> { model1, model2}, result);
            AssertJsonEquals(new List<DummyModel> { model1, model2 }.OrderBy(x => x.Id),
                (await _repository.For<DummyModel>().GetAll(model1.Id, model2.Id)).OrderBy(x => x.Id));
        }
        
        [Fact]
        public async Task Test__Get()
        {
            // Arrange
            var model = new DummyModel
            {
                Name = "Foo", Children = new List<Nested>
                {
                    new Nested()
                }
            };

            // Act
            await _repository.For<DummyModel>().Save(model);

            // Assert
            AssertJsonEquals(model, await _repository.For<DummyModel>().Get(model.Id));
        }
        
        [Fact]
        public async Task Test__GetWhere()
        {
            // Arrange
            var model = new DummyModel
            {
                Name = "Foo", Children = new List<Nested>
                {
                    new Nested()
                }
            };

            // Act
            await _repository.For<DummyModel>().Save(model);
            
            // Assert
            AssertJsonEquals(model, await _repository.For<DummyModel>().Get(x => x.Id == model.Id));
        }
        
        [Fact]
        public async Task Test__GetAll()
        {
            // Arrange
            var model = new DummyModel
            {
               Name = "Foo", Children = new List<Nested>
                {
                    new Nested()
                }
            };
            await _repository.For<DummyModel>().Save(model);
            
            // Act
            var result = await _repository.For<DummyModel>().GetAll();
            
            // Assert
            AssertJsonEquals(new List<DummyModel> { model }, result);
        }
        
        [Fact]
        public async Task Test__GetAllIds()
        {
            // Arrange
            var model = new DummyModel
            {
                Name = "Foo", Children = new List<Nested>
                {
                    new Nested()
                }
            };
            await _repository.For<DummyModel>().Save(model);
            
            // Act
            var result = await _repository.For<DummyModel>().GetAll(model.Id);
            
            // Assert
            AssertJsonEquals(new List<DummyModel> { model }, result);
        }
        
        [Fact]
        public async Task Test__GetAllWhere()
        {
            // Arrange
            var model = new DummyModel
            {
                Name = "Foo", Children = new List<Nested>
                {
                    new Nested()
                }
            };
            await _repository.For<DummyModel>().Save(model);
            
            // Act
            var result = await _repository.For<DummyModel>().GetAll(x => x.Id == model.Id);
            
            // Assert
            AssertJsonEquals(new List<DummyModel> { model}, result);
        }

        [Fact]
        public async Task Test__Update()
        {
            // Arrange
            var model = new DummyModel
            {
                 Name = "Foo", Children = new List<Nested>
                {
                    new Nested()
                }
            };

            await _repository.For<DummyModel>().Save(model);
            
            // Act
            model.Name = "Bar";
            await _repository.For<DummyModel>().Update(model.Id, model);
            
            // Assert
            AssertJsonEquals(model, await _repository.For<DummyModel>().Get(model.Id));
        }
        
        
        [Fact]
        public async Task Test__Update_Action()
        {
            // Arrange
            var model = new DummyModel
            {
                Name = "Foo", Children = new List<Nested>
                {
                    new Nested()
                }
            };

            await _repository.For<DummyModel>().Save(model);
            
            // Act
            await _repository.For<DummyModel>().Update(model.Id, x =>
            {
                x.Name = "Bad";
            });
            
            // Assert
            AssertJsonEquals(model, await _repository.For<DummyModel>().Get(model.Id));
        }
        
        [Fact]
        public async Task Test__UpdateWhere()
        {
            // Arrange
            var model = new DummyModel
            {
               Name = "Foo", Children = new List<Nested>
                {
                    new Nested()
                }
            };

            await _repository.For<DummyModel>().Save(model);
            
            // Act
            model.Name = "Bar";
            await _repository.For<DummyModel>().Update(model, x => x.Id == model.Id);
            
            // Assert
            AssertJsonEquals(model, await _repository.For<DummyModel>().Get(x => x.Id == 1));
        }
        
        [Fact]
        public async Task Test__Delete()
        {
            // Arrange
            var model = new DummyModel
            {
                 Name = "Foo", Children = new List<Nested>
                {
                    new Nested()
                }
            };

            await _repository.For<DummyModel>().Save(model);
            
            // Act
            await _repository.For<DummyModel>().Delete(model.Id);
            
            // Assert
            Assert.Empty(await _repository.For<DummyModel>().GetAll());
        }
        
        [Fact]
        public async Task Test__DeleteWhere()
        {
            // Arrange
            var model = new DummyModel
            {
                Name = "Foo", Children = new List<Nested>
                {
                    new Nested()
                }
            };

            await _repository.For<DummyModel>().Save(model);
            
            // Act
            await _repository.For<DummyModel>().Delete(x => x.Id == model.Id);
            
            // Assert
            Assert.Empty(await _repository.For<DummyModel>().GetAll());
        }
        
        [Fact(Skip = "For some reason, LINQ get method is eager loading first level dependencies")]
        public async Task Test__LightSession()
        {
            // Arrange
            var model = new DummyModel
            {
                Name = "Foo", Children = new List<Nested>()
            };

            await _repository.For<DummyModel>().Save(model);

            var nested = new Nested
            {
                ParentRef = model
            };

            await _repository.For<Nested>().Save(nested);
            
            // Act
            var entity = await _repository.For<DummyModel>().Light().Get(x => x.Id == model.Id);
            
            // Assert
            Assert.Null(entity.Children);
        }
        
        [Fact]
        public async Task Test__MultipleFilterExpr()
        {
            // Arrange
            var model1 = new DummyModel
            {
                Name = "Foo1", Children = new List<Nested>()
            };
            
            var model2 = new DummyModel
            {
                Name = "Foo2", Children = new List<Nested>()
            };

            await _repository.For<DummyModel>().Save(model1, model2);

            // Act
            var entity = await _repository.For<DummyModel>().Light().Get(x => x.Name != "Foo1", x => x.Name == "Foo2");
            
            // Assert
            Assert.Equal("Foo2", entity.Name);
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            await using var repo = _repository.For<DummyModel>().Delayed();
            var models = await repo.GetAll();
            
            foreach (var model in models)
            {
                await repo.Delete(model.Id);
            }
        }

        private static void AssertJsonEquals<T>(T expected, T actual)
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            var expectedJson = JsonConvert.SerializeObject(expected, jsonSetting);
            var actualJson = JsonConvert.SerializeObject(actual, jsonSetting);

            Assert.Equal(expectedJson, actualJson);
        }
    }
}