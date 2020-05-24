# EF.Core.Repository

Simple repository for Ef.Core with basic CRUD functionality

### How to use

- Entity should implement `IEntity<TSource, TId>`
```c#
public class DummyModel : IEntity<DummyModel, int>
{
    public int Id { get; set; }
    
    public string Name { get; set; }

    public List<Nested> Children { get; set; }
}
```

- Create profile

```c#
public class DummyModelProfile : IEntityProfile<DummyModel, int> 
{
    private readonly IEntityProfileAuxiliary<Nested, int> _auxiliary;

    public DummyModelProfile(IEntityProfileAuxiliary<Nested, int> auxiliary)
    {
        _auxiliary = auxiliary;
    }

    public DummyModel Update(DummyModel entity, DummyModel dto)
    {
        entity.Name = dto.Name;
        entity.Children = _auxiliary.ModifyList(entity.Children, dto.Children);

        return entity;
    }

    public IQueryable<DummyModel> Include<TQueryable>(TQueryable queryable) where TQueryable : IQueryable<DummyModel>
    {
        return queryable;
    }
}
```

- Register dependency

```c#
var serviceProvider = services
    .AddEfRepository<EntityDbContext>(options => options
        .Profiles(Assembly.GetExecutingAssembly()));
```

- Use `IBasicCrud`
```c#
IEfRepository repo = ... // DI inject instance
IBasicCrud<DummyModel> = repo.For<DummyModel>();
```

- Available methods
```c#
Task<IEnumerable<TSource>> GetAll();

Task<TSource> Get(id);

Task<TSource> Save(instance);

Task<TSource> Delete(id);

Task<TSource> Update(id, dto);
```