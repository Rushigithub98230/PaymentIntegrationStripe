using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentIntegrationStripe.Domain.Payments;

namespace PaymentIntegrationStripe.Infrastructure.Persistence.Configurations;

public sealed class StripeWebhookEventConfiguration : IEntityTypeConfiguration<StripeWebhookEvent>
{
    public void Configure(EntityTypeBuilder<StripeWebhookEvent> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StripeEventId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EventType).HasMaxLength(150).IsRequired();
        builder.Property(x => x.PayloadHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Processed).IsRequired();
        builder.Property(x => x.AttemptCount).IsRequired();
        builder.Property(x => x.LastError).HasMaxLength(4000);
        builder.Property(x => x.ReceivedAtUtc).IsRequired();
        builder.HasIndex(x => x.StripeEventId).IsUnique();
        builder.HasIndex(x => new { x.Processed, x.ReceivedAtUtc });
    }
}
