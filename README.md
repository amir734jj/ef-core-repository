# EF.Core.Repository

Simple repository for Ef.Core with basic CRUD functionality

### How to use

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
public class DummyModelProfile : IEntityProfile<DummyModel, int> 
{
    private readonly IEntityProfileAuxiliary<Nested, int> _auxiliary;

    // Optionally inject utility for list add/delete
    public DummyModelProfile(IEntityProfileAuxiliary<Nested, int> auxiliary)
    {
        _auxiliary = auxiliary;
    }

    public DummyModel Update(DummyModel entity, DummyModel dto)
    {
        entity.Name = dto.Name;

        // ModifyList will try to add/delete entities based on Id based on whether they
        // appear in dto.Children or not 
        entity.Children = _auxiliary.ModifyList(entity.Children, dto.Children);

        // Return entity or response
        // This will be used as a return value for update method in BasicCrud
        return entity;
    }

    // Intercept IQueryable to include related entities
    public IQueryable<DummyModel> Include<TQueryable>(TQueryable queryable) where TQueryable : IQueryable<DummyModel>
    {
        return queryable.Children();
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
IBasicCrud<DummyModel> = repo.For<DummyModel>();
```

- Available methods in `IBasicCrud`
```c#
Task<IEnumerable<TSource>> GetAll();

Task<TSource> Get(id);

Task<TSource> Save(dto);

Task<TSource> Delete(id);

Task<TSource> Update(id, dto);
```
