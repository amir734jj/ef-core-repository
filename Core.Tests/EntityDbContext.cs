using Core.Tests.Models;
using Microsoft.EntityFrameworkCore;

namespace Core.Tests
{
    public sealed class EntityDbContext : DbContext
    {
        public DbSet<DummyModel> DummyModels { get; set; }
        
        public DbSet<NestedModel> Nesteds { get; set; }

        public EntityDbContext(DbContextOptions<EntityDbContext> options): base(options)
        {
            Database.EnsureCreated();
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EntityDbContext).Assembly);
        }
    }
}