using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentIntegrationStripe.Domain.Payments;

namespace PaymentIntegrationStripe.Infrastructure.Persistence.Configurations;

public sealed class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.ProviderRefundId).HasMaxLength(200);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.HasIndex(x => x.ProviderRefundId).IsUnique().HasFilter("[ProviderRefundId] IS NOT NULL");
        builder.HasIndex(x => new { x.PaymentId, x.CreatedAtUtc });
    }
}
