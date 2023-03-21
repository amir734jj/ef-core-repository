using Core.Tests.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Tests.Entities;

public class DummyModelEntity : IEntityTypeConfiguration<DummyModel>
{
    public void Configure(EntityTypeBuilder<DummyModel> builder)
    {
        builder.HasMany(x => x.Children)
            .WithOne(x => x.ParentRef)
            .HasForeignKey(x => x.ParentRefId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}