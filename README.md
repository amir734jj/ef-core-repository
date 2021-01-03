# EF.Core.Repository

Simple repository for Ef.Core with basic CRUD functionality
The reason I implemented this is because I found myself re-writing basic CRUD functionality over and over again. Most of the time:
- use `.include(...)` to include eager load associated entities
- update (add/delete/update) children properties by Id

[![NuGet Status](https://img.shields.io/nuget/v/SimpleEfCoreRepository.svg)](https://www.nuget.org/packages/SimpleEfCoreRepository/)
[![Build Status](https://travis-ci.com/amir734jj/ef-core-repository.svg?branch=master)](https://travis-ci.com/amir734jj/ef-core-repository)

#### Basic setup

- Entity should implement `IEntity<TId>`
```c#
public class DummyModel : IEntity<int>
{
    public int Id { get; set; }
    
    public string Name { get; set; }

    public List<Nested> Children { get; set; }
}
```

- Create profile which is used to update an entity given a DTO

```c#
public class DummyModelProfile : IEntityProfile<DummyModel> 
{
    private readonly IEntityProfileAuxiliary _auxiliary;

    // Optionally inject this utility for list add/delete
    public DummyModelProfile(IEntityProfileAuxiliary auxiliary)
    {
        _auxiliary = auxiliary;
    }

    public void Update(DummyModel entity, DummyModel dto)
    {
        entity.Name = dto.Name;

        // ModifyList will try to add/delete entities based on Id based on whether they
        // appear in dto.Children or not 
        entity.Children = _auxiliary.ModifyList<Nested, int>(entity.Children, dto.Children);
    }

    // Intercept IQueryable to include related entities
    public IQueryable<DummyModel> Include<TQueryable>(TQueryable queryable) where TQueryable : IQueryable<DummyModel>
    {
        return queryable.Include(x => x.Children);
    }
}
```

- Register dependency via `IServiceCollection` extension

```c#
var serviceProvider = services
    .AddEfRepository<EntityDbContext>(options => options
        .Profiles(Assembly.GetExecutingAssembly()));
```

- Use `IBasicCrud`
```c#
IEfRepository repo = ... // DI inject IEfRepository

// Get IBasicCrud instance
IBasicCrud<DummyModel> = repo.For<DummyModel>();
```

- Available methods in `IBasicCrud` or `IBasicCrudType`
```c#
// Get all entities without any filter
Task<IEnumerable<TSource>> GetAll();

// Get all entities given an array of Ids
Task<IEnumerable<TSource>> GetAll<TId>(param TId[] ids);

// Get all entities given a filter expression
Task<IEnumerable<TSource>> GetAll(Expression<Func<TSource, bool>>);

// Get single entity by Id
Task<TSource> Get(id);

// Get single entity given a filter expression
Task<TSource> Get(Expression<Func<TSource, bool>>);

// Save entity
Task<TSource> Save(dto);

// Save multiple entities
Task<IEnumerable<TSource>> Save(param TSource[] dtos);

// Update entity by Id
Task<TSource> Update(id, dto);

// Update entity by filter expression
Task<TSource> Update(Expression<Func<TSource, bool>>, dto);

// Delete entity by Id
Task<TSource> Delete(id);

// Delete entity given a filter expression
Task<TSource> Delete(Expression<Func<TSource, bool>>);

// This is useful if you want to defer SaveChanges in session mode
// Changes are not automatically saved back to DbContext in session mode
IBasicCrud<TSource, TId> Session();
```

Notes:

- "Id" should by a  
