using System.Collections.Generic;
using Core.Tests.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Core.Tests
{
    public sealed class EntityDbContext : DbContext
    {
        public DbSet<DummyModel> DummyModels { get; set; }
        
        public DbSet<NestedModel> Nesteds { get; set; }

        public DbSet<TaggedModel> TaggedModels { get; set; }

        public EntityDbContext(DbContextOptions<EntityDbContext> options): base(options)
        {
            Database.EnsureCreated();
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EntityDbContext).Assembly);

            modelBuilder.Entity<TaggedModel>(e =>
            {
                e.Property(x => x.Tags)
                    .HasConversion(
                        v => JsonConvert.SerializeObject(v),
                        v => JsonConvert.DeserializeObject<List<string>>(v) ?? new List<string>())
                    .HasColumnType("TEXT");
            });
        }
    }
}