using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentIntegrationStripe.Domain.Payments;

namespace PaymentIntegrationStripe.Infrastructure.Persistence.Configurations;

public sealed class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttempt>
{
    public void Configure(EntityTypeBuilder<PaymentAttempt> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AttemptNumber).IsRequired();
        builder.Property(x => x.IdempotencyKey).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ProviderPaymentIntentId).HasMaxLength(200);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.FailureCode).HasMaxLength(200);
        builder.Property(x => x.FailureMessage).HasMaxLength(2000);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.HasIndex(x => new { x.PaymentId, x.AttemptNumber }).IsUnique();
        builder.HasIndex(x => x.IdempotencyKey).IsUnique();
        builder.HasIndex(x => x.ProviderPaymentIntentId).IsUnique().HasFilter("[ProviderPaymentIntentId] IS NOT NULL");
    }
}
