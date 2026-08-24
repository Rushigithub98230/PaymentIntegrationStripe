using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentIntegrationStripe.Domain.Orders;

namespace PaymentIntegrationStripe.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Sku).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ProductName).HasMaxLength(250).IsRequired();
        builder.Property(x => x.UnitPrice).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.Quantity).IsRequired();
        builder.HasIndex(x => new { x.OrderId, x.ProductVariantId });
        builder.HasOne<PaymentIntegrationStripe.Domain.Catalog.ProductVariant>().WithMany().HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
    }
}
