using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentIntegrationStripe.Domain.Payments;

namespace PaymentIntegrationStripe.Infrastructure.Persistence.Configurations;

public sealed class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.ProviderTransactionId).HasMaxLength(200);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.HasIndex(x => x.ProviderTransactionId).IsUnique().HasFilter("[ProviderTransactionId] IS NOT NULL");
        builder.HasIndex(x => new { x.PaymentId, x.CreatedAtUtc });
    }
}
