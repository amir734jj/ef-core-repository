using Core.Tests.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Tests.Entities;

public class NestedModelEntity : IEntityTypeConfiguration<NestedModel>
{
    public void Configure(EntityTypeBuilder<NestedModel> builder)
    {
        builder.HasOne(x => x.ParentRef)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentRefId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}