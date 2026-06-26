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

##### Optional profiles (`DefaultProfiles`)

Writing an `EntityProfile<T>` per entity is optional. Call `DefaultProfiles()` and every entity type
exposed by the `DbContext` (every `DbSet<T>`, including non-public/scaffolded ones) that has **no**
explicit profile automatically gets a default profile. Entities with a discoverable key get one that
**maps all properties** (`MapAll`) and adds **no eager includes**; keyless entities (e.g. database
views) get an **empty** profile so they stay usable for reads, inserts and filter-based updates.
Explicit profiles always win, so you can mix the two. This is especially handy for DB-first /
scaffolded models where hand-writing profiles is tedious:

```c#
services.AddEfRepository<ScaffoldedDbContext>(options => options
    .Profile(Assembly.GetExecutingAssembly())          // explicit profiles (optional)
    .DefaultProfiles());                                // default profile for the rest
```

> By-id operations (`Get<TId>`, `Update<TId>`, `Delete<TId>`) still require a discoverable key —
> either a `[Key]` attribute or a property named `id`/`_id`. Keyless entities can only be used
> through the filter- and query-based methods.

- Use `IBasicCrud`
```c#
IEfRepository repo = ... // DI inject IEfRepository

// Get IBasicCrud instance
IBasicCrud<DummyModel> = repo.For<DummyModel>();
```

#### Factory pattern for parallel queries

EF Core's `DbContext` is not thread-safe. If you need to run multiple queries in parallel, use `IDbContextFactory` integration. Each created `IBasicCrud<T>` gets its own `DbContext`.

- Register using `AddDbContextFactory` + `AddEfRepositoryFactory`

```c#
// This registers IDbContextFactory<EntityDbContext> as Singleton
// and EntityDbContext as Scoped
builder.Services.AddDbContextFactory<EntityDbContext>(options =>
    options.UseNpgsql(connectionString));

// This registers:
//   - IEfRepository (scoped) for standard usage
//   - IBasicCrud<T> (scoped) for standard usage
//   - IEfRepositoryCreator<T> (singleton) for parallel usage
builder.Services.AddEfRepositoryFactory<EntityDbContext>(options =>
    options.Profile(Assembly.GetExecutingAssembly()));
```

- Inject `IEfRepositoryCreator<T>` and create short-lived `IBasicCrud<T>` instances

```c#
public class DashboardService(
    IEfRepositoryCreator<Order> orderCreator,
    IEfRepositoryCreator<Log> logCreator)
{
    public async Task<DashboardDto> GetDashboard(int userId)
    {
        // Run queries in parallel - each gets its own DbContext
        var ordersTask = GetOrdersAsync(userId);
        var logsTask = GetLogsAsync();

        await Task.WhenAll(ordersTask, logsTask);

        return new DashboardDto(await ordersTask, await logsTask);
    }

    private async Task<IEnumerable<Order>> GetOrdersAsync(int userId)
    {
        // Create a fresh IBasicCrud with its own DbContext
        await using var orders = await orderCreator.CreateAsync();
        return await orders.GetAll(filterExprs: [o => o.UserId == userId]);
    }

    private async Task<IEnumerable<Log>> GetLogsAsync()
    {
        await using var logs = await logCreator.CreateAsync();
        return await logs.GetAll();
    }
}
```

> **Note:** The standard `IBasicCrud<T>` (scoped) and `IEfRepository` registrations still work alongside the factory. Use `IEfRepositoryCreator<T>` only when you need parallel query execution.

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

// Update first entity matching a filter expression
Task<TSource> Update(Expression<Func<TSource, bool>>[], Action<TSource>);

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

// Join another entity set; continue querying on the read-only Joined<TSource, TInner> surface
IReadOnlyCrud<Joined<TSource, TInner>> Join<TInner, TKey>(
    Expression<Func<TSource, TKey>>, Expression<Func<TInner, TKey>>, JoinType = JoinType.Inner);
```

#### Querying & joins

`IBasicCrud<T>` also exposes a read-only surface, `IReadOnlyCrud<T>`, covering the query operations
(`Get`, `GetAll`, `Any`, `Count`, `Take`, `Join`). A consumer that only needs to read can depend on
`IReadOnlyCrud<T>` instead of the full CRUD interface - and, importantly, it keeps **join results
query-only by construction** (there is nothing to guard against because the type simply has no
write methods).

##### Join

`Join` lets you join two entity sets that have **no navigation property** between them. It returns an
`IReadOnlyCrud<Joined<TOuter, TInner>>`, so the usual `GetAll`/`Get`/`Any`/`Count` surface - with
`filterExprs` and `project` - applies directly to the joined rows. The underlying `IQueryable` is never
exposed.

```c#
// repo.For<Order>() returns IBasicCrud<Order>
var rows = await repo.For<Order>()
    .Join<Customer, int>(o => o.CustomerId, c => c.Id, JoinType.Inner)
    .GetAll(
        filterExprs: [pair => pair.Outer.Total > 100],
        orderBy:     Ordering<Joined<Order, Customer>>.Desc(pair => pair.Outer.CreatedAt),
        project:     pair => new OrderSummary
        {
            OrderId      = pair.Outer.Id,
            CustomerName = pair.Inner.Name,
        });
```

`GetAll` also accepts an optional `distinctBy` key selector. When supplied, the query keeps one row
per distinct key (`GroupBy(key).Select(g => g.First())`); when `null` (the default) no distincting is
applied:

```c#
// One order per customer name.
var rows = await repo.For<Order>()
    .GetAll(distinctBy: o => o.CustomerName);
```

> **Note:** `distinctBy` is composed as a `GroupBy`/`First` on the `IQueryable`. Whether it runs
> server-side depends on your provider's translation support; verify if you rely on it.

##### Ordering

`GetAll` takes an optional `Ordering<TSource>` — a fluent, multi-key sort. Start with `Asc` / `Desc`,
then chain `ThenAsc` / `ThenDesc` for secondary keys. The first key becomes `ORDER BY` and each
subsequent key a `THEN BY`, keeping its own direction:

```c#
// ORDER BY LastName ASC, CreatedAt DESC
var rows = await repo.For<Customer>().GetAll<Customer>(
    orderBy: Ordering<Customer>.Asc(c => c.LastName).ThenDesc(c => c.CreatedAt));
```

Each joined row is a `Joined<TOuter, TInner>` exposing `.Outer` and `.Inner`. For outer joins the
unmatched side is `null`. `JoinType` supports:

| `JoinType` | SQL | Unmatched side |
| --- | --- | --- |
| `Inner` (default) | `INNER JOIN` | row excluded |
| `Left` | `LEFT JOIN` | `Inner` is `null` |
| `Right` | `RIGHT JOIN` | `Outer` is `null` |
| `FullOuter` | full outer join (emitted as a `UNION ALL`) | either side `null` |

> **Note:** `FullOuter` is composed as a `UNION ALL` of the left join and the unmatched-right rows.
> Most providers translate this to SQL; verify against your provider if you rely on it.

##### Exclusive joins

`Join` also accepts a `JoinInclusivity` argument that composes with `JoinType`. The default,
`Inclusive`, keeps every row the join produces. `Exclusive` keeps only the rows that exist on a
**single side** - the outer crescents of the join Venn diagram:

| `JoinType` + `JoinInclusivity.Exclusive` | Keeps | Set notation |
| --- | --- | --- |
| `Left` | outer rows with no inner match | `A AND NOT B` |
| `Right` | inner rows with no outer match | `B AND NOT A` |
| `FullOuter` | rows present on exactly one side | `A XOR B` |

```c#
// Orders that have no matching customer (left-only rows).
var orphans = await repo.For<Order>()
    .Join<Customer, int>(o => o.CustomerId, c => c.Id, JoinType.Left, JoinInclusivity.Exclusive)
    .GetAll(project: pair => pair.Outer);
```

> **Note:** `Inner` has no exclusive region (its result is always matched on both sides), so
> `JoinType.Inner` with `JoinInclusivity.Exclusive` throws an `ArgumentException`.

Joins are chainable - the read-only result itself exposes `Join`, keyed off the `Joined<,>` pair:

```c#
var rows = await repo.For<Order>()
    .Join<Customer, int>(o => o.CustomerId, c => c.Id)        // -> Joined<Order, Customer>
    .Join<Address, int>(pair => pair.Inner.AddressId, a => a.Id) // -> Joined<Joined<Order, Customer>, Address>
    .GetAll(project: pair => new { pair.Outer.Outer.Id, City = pair.Inner.City });
```

Everything here is an interface (`IBasicCrud<T>`, `IReadOnlyCrud<T>`), so it all mocks cleanly in unit
tests; the concrete implementations are internal.

Notes:

- "Id" has a type constraint of `: struct`, which means it accepts all primitive types including `GUID` and `String`.
- Including all associated entities will lead to "god object" which is *anti-pattern*. However, you can use `LightWeight` session to not include all dependent properties.
