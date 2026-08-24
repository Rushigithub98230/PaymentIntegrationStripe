using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentIntegrationStripe.Domain.Payments;

namespace PaymentIntegrationStripe.Infrastructure.Persistence.Configurations;

public sealed class RefundAttemptConfiguration : IEntityTypeConfiguration<RefundAttempt>
{
    public void Configure(EntityTypeBuilder<RefundAttempt> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AttemptNumber).IsRequired();
        builder.Property(x => x.IdempotencyKey).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ProviderRefundId).HasMaxLength(200);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.HasIndex(x => new { x.RefundId, x.AttemptNumber }).IsUnique();
        builder.HasIndex(x => x.IdempotencyKey).IsUnique();
        builder.HasIndex(x => x.ProviderRefundId).IsUnique().HasFilter("[ProviderRefundId] IS NOT NULL");
    }
}
