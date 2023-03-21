# EF.Core.Repository

Simple repository for Ef.Core with basic CRUD functionality
The reason I implemented this is because I found myself re-writing basic CRUD functionality over and over again. Most of the time:
- use `.include(...)` to include eager load associated entities
- update (add/delete/update) children properties

Using repository pattern with entity framework enforces a consistent convention and that is what this library is aiming towards.

[![NuGet Status](https://img.shields.io/nuget/v/SimpleEfCoreRepository.svg)](https://www.nuget.org/packages/SimpleEfCoreRepository/)
[![Build Status](https://travis-ci.com/amir734jj/ef-core-repository.svg?branch=master)](https://travis-ci.com/amir734jj/ef-core-repository)

#### Basic setup

- Entity should have Id property. Using `[Key]` is optional **IF** Id property does not follow common naming convention (i.e. `id` or `_id`, case insensitive)
```c#
public class DummyModel
{
    public string Name { get; set; }

    public List<Nested> Children { get; set; }
}
```

- Create profile which is used to update an entity given a DTO

```c#
public class DummyModelProfile : EntityProfile<DummyModel> 
{
    public override void Update(DummyModel entity, DummyModel dto)
    {
        entity.Name = dto.Name;

        // ModifyList will try to add/delete entities based on Id based on whether they
        // appear in dto.Children or not 
        ModifyList(entity.Children, dto.Children, x => x.Id);
    }

    // Intercept IQueryable to include related entities
    public override IQueryable<DummyModel> Include<TQueryable>(TQueryable queryable) where TQueryable : IQueryable<DummyModel>
    {
        return queryable.Include(x => x.Children);
    }
}
```

- OR use an auto mapper functionality to map properties. Be careful when using auto mapper. I recommend using manual mapper for more control.

```c#
public class DummyModelProfile : EntityProfile<DummyModel> 
{
    public DummyModelProfile()
    {
        // Map indivisually
        Map(x => x.Name);
        Map(x => x.Children);
        // OR
        MapAll();
    }

    // Intercept IQueryable to include related entities
    public override IQueryable<DummyModel> Include<TQueryable>(TQueryable queryable) where TQueryable : IQueryable<DummyModel>
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

- Available methods in `IBasicCrud`
```c#
// Get all entities given an array of Ids
Task<IEnumerable<TSource>> GetAll<TId>(param TId[]);

// Get all entities given a filter expression
Task<IEnumerable<TSource>> GetAll(params Expression<Func<TSource, bool>>[]);

// Boolean if there is any
Task<bool> Any(params Expression<Func<TSource, bool>>[]);

// Get single entity by Id
Task<TSource> Get<TId>(TId);

// Get single entity given a filter expression
Task<TSource> Get(params Expression<Func<TSource, bool>>[]);

// Save one or more entities
Task<IEnumerable<TSource>> Save(TSource[]);

// Update entity by Id
Task<TSource> Update<TId>(TId, TSource);

// Update entity manually
Task<TSource> Update<TId>(TId, Action<TSource>);

// Update entity by filter expression
Task<TSource> Update(TSource, params Expression<Func<TSource, bool>>[]);

// Update entity manually by filter expression
Task<TSource> Update(Action<TSource>, params Expression<Func<TSource, bool>>[]);

// Delete entity by Id
Task<IEnumerable<TSource>> Delete<TId>(TId[]);

// Delete entity given a filter expression
Task<TSource> Delete(params Expression<Func<TSource, bool>>[]);

// This is useful if you want to defer SaveChanges in session mode
// Changes are not automatically saved back to DbContext in session mode
IBasicCrud<TSource, TId> Delayed();

// This is useful if you want don't want to include all related entities
// or turn entities into a "god" object
IBasicCrud<TSource, TId> Light();
```

Notes:

- "Id" has a type constraint of `: struct`, which means it accepts all primitive types including `GUID` and `String`.
- Including all associated entities will lead to "god object" which is *anti-pattern*. However, you can use `LightWeight` session to not include all dependent properties.
