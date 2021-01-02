using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Core.Tests.Models;
using EfCoreRepository.Extensions;
using EfCoreRepository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
                .AddDbContext<EntityDbContext>(x => x.UseInMemoryDatabase("test"))
                .AddEfRepository<EntityDbContext>(options => options
                    .Profiles(Assembly.GetExecutingAssembly()))
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
            var result = await _repository.For<DummyModel>().Save(model);
            
            // Assert
            Assert.Equal(model, result);
            Assert.Equal(model, await _repository.For<DummyModel>().Get(model.Id));
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
            var result = await _repository.For<DummyModel>().Save(model1, model2);
            
            // Assert
            Assert.Equal(new List<DummyModel> { model1, model2}, result);
            Assert.Equal(new List<DummyModel> { model1, model2},  await _repository.For<DummyModel>().GetAll(model1.Id, model2.Id));
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
            Assert.Equal(model, await _repository.For<DummyModel>().Get(model.Id));
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
            Assert.Equal(model, await _repository.For<DummyModel>().Get(x => x.Id == model.Id));
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
            Assert.Equal(new List<DummyModel> { model }, result);
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
            Assert.Equal(new List<DummyModel> { model }, result);
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
            Assert.Equal(new List<DummyModel> { model}, result);
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
            Assert.Equal(model, await _repository.For<DummyModel>().Get(model.Id));
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
            await _repository.For<DummyModel>().Update(x => x.Id == model.Id, model);
            
            // Assert
            Assert.Equal(model, await _repository.For<DummyModel>().Get(x => x.Id == 1));
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

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            await using var repo = _repository.For<DummyModel>().Session();
            
            var models = await repo.GetAll();
            
            foreach (var model in models)
            {
                await repo.Delete(model.Id);
            }
        }
    }
}