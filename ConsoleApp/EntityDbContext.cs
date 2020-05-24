using ConsoleApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsoleApp
{
    public sealed class EntityDbContext : DbContext
    {
        public DbSet<DummyModel> DummyModels { get; set; }

        public EntityDbContext(DbContextOptions<EntityDbContext> options): base(options)
        {
            Database.EnsureCreated();
        }
    }
}