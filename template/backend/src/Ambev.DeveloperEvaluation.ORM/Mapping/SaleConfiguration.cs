using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.SaleNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(s => s.SaleNumber).IsUnique();

        builder.Property(s => s.SaleDate).IsRequired();

        builder.Property(s => s.CustomerExternalId).IsRequired().HasMaxLength(100);
        builder.Property(s => s.CustomerName).IsRequired().HasMaxLength(200);
        builder.HasIndex(s => s.CustomerExternalId);

        builder.Property(s => s.BranchExternalId).IsRequired().HasMaxLength(100);
        builder.Property(s => s.BranchName).IsRequired().HasMaxLength(200);
        builder.HasIndex(s => s.BranchExternalId);

        builder.Property(s => s.TotalAmount).HasPrecision(18, 2);

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasMany(s => s.Items)
            .WithOne()
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
