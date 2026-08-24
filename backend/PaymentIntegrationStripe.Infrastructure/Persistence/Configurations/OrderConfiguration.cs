using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentIntegrationStripe.Domain.Identity;
using PaymentIntegrationStripe.Domain.Orders;

namespace PaymentIntegrationStripe.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Subtotal).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.Discount).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.Tax).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.Shipping).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.Total).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.HasIndex(x => new { x.CustomerId, x.CreatedAtUtc });
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
    }
}
