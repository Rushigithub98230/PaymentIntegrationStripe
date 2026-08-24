using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentIntegrationStripe.Domain.Payments;

namespace PaymentIntegrationStripe.Infrastructure.Persistence.Configurations;

public sealed class PaymentStatusHistoryConfiguration : IEntityTypeConfiguration<PaymentStatusHistory>
{
    public void Configure(EntityTypeBuilder<PaymentStatusHistory> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PreviousStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.NewStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Source).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.HasIndex(x => new { x.PaymentId, x.CreatedAtUtc });
    }
}
