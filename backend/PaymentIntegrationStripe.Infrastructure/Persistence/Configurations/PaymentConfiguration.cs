using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentIntegrationStripe.Domain.Orders;
using PaymentIntegrationStripe.Domain.Payments;

namespace PaymentIntegrationStripe.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.IdempotencyKey).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ProviderCustomerId).HasMaxLength(200);
        builder.Property(x => x.ProviderPaymentIntentId).HasMaxLength(200);
        builder.Property(x => x.ProviderChargeId).HasMaxLength(200);
        builder.Property(x => x.AmountRefunded).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.LastFailureCode).HasMaxLength(200);
        builder.Property(x => x.LastFailureMessage).HasMaxLength(2000);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();

        builder.HasIndex(x => x.IdempotencyKey).IsUnique();
        builder.HasIndex(x => x.ProviderPaymentIntentId).IsUnique().HasFilter("[ProviderPaymentIntentId] IS NOT NULL");
        builder.HasIndex(x => x.ProviderChargeId).IsUnique().HasFilter("[ProviderChargeId] IS NOT NULL");
        builder.HasIndex(x => new { x.OrderId, x.CreatedAtUtc });

        builder.HasOne<Order>()
            .WithOne()
            .HasForeignKey<Payment>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany<PaymentAttempt>().WithOne().HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany<PaymentTransaction>().WithOne().HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany<PaymentStatusHistory>().WithOne().HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany<Refund>().WithOne().HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Restrict);
    }
}
