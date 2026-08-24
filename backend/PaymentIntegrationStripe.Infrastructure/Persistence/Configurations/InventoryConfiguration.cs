using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentIntegrationStripe.Domain.Catalog;

namespace PaymentIntegrationStripe.Infrastructure.Persistence.Configurations;

public sealed class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AvailableQuantity).IsRequired();
        builder.Property(x => x.ReservedQuantity).IsRequired();
        builder.HasIndex(x => x.ProductVariantId).IsUnique();
        builder.HasOne<ProductVariant>().WithMany().HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
    }
}
