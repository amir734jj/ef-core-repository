# EF.Core.Repository

Simple repository for Ef.Core with basic CRUD functionality
The reason I implemented this is because I found myself re-writing basic CRUD functionality over and over again. Most of the time:
- use `.include(...)` to include eager load associated entities
- update (add/delete/update) children properties
- 

Using repository pattern with entity framework enforces a consistent convention and that is what this library is aiming towards.

[![NuGet Status](https://img.shields.io/nuget/v/SimpleEfCoreRepository.svg)](https://www.nuget.org/packages/SimpleEfCoreRepository/)
[![Build Status](https://travis-ci.com/amir734jj/ef-core-repository.svg?branch=master)](https://travis-ci.com/amir734jj/ef-core-repository)

#### Basic setup

- Entity should have Id property. Using `[Key]` is optional if Id property does not follow common naming convention.
```c#
public class DummyModel
{
    public string Name { get; set; }

    public List<Nested> Children { get; set; }
}
```

- Create profile which is used to update an entity given a DTO

```c#
public class DummyModelProfile : IEntityProfile<DummyModel> 
{
    public void Update(DummyModel entity, DummyModel dto)
    {
        entity.Name = dto.Name;

        // ModifyList will try to add/delete entities based on Id based on whether they
        // appear in dto.Children or not 
        ModifyList(entity.Children, dto.Children, x => x.Id);
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
        .Profile(Assembly.GetExecutingAssembly()));
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
Task<IEnumerable<TSource>> GetAll<TId>(param TId[]);

// Get all entities given a filter expression
Task<IEnumerable<TSource>> GetAll(Expression<Func<TSource, bool>>);

// Get single entity by Id
Task<TSource> Get<TId>(TId);

// Get single entity given a filter expression
Task<TSource> Get(Expression<Func<TSource, bool>>);

// Save entity
Task<TSource> Save(TSource);

// Save multiple entities
Task<IEnumerable<TSource>> Save(param TSource[]);

// Update entity by Id
Task<TSource> Update<TId>(TId, TSource);

// Update entity by filter expression
Task<TSource> Update(Expression<Func<TSource, bool>>, TSource);

// Delete entity by Id
Task<TSource> Delete<TId>(TId);

// Delete entity given a filter expression
Task<TSource> Delete(Expression<Func<TSource, bool>>);

// This is useful if you want to defer SaveChanges in session mode
// Changes are not automatically saved back to DbContext in session mode
IBasicCrud<TSource, TId> Delayed();

// This is useful if you want don't want to include all related entities
// or turn entities into a "god" object
IBasicCrud<TSource, TId> Light();
```

Notes:

- "Id" has a type constraint of `: struct`, which means it accepts all primitive types including `GUID` and `String`.
- Including all associated entities will lead to "god object" which is *anti-pattern*. I recommend using [this library](https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor) as L2 cache
