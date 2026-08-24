using Microsoft.EntityFrameworkCore;
using PaymentIntegrationStripe.Domain.Payments;

namespace PaymentIntegrationStripe.Infrastructure.Persistence;

public sealed class PaymentIntegrationDbContext(DbContextOptions<PaymentIntegrationDbContext> options)
    : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentAttempt> PaymentAttempts => Set<PaymentAttempt>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<PaymentStatusHistory> PaymentStatusHistory => Set<PaymentStatusHistory>();
    public DbSet<Refund> Refunds => Set<Refund>();
    public DbSet<RefundAttempt> RefundAttempts => Set<RefundAttempt>();
    public DbSet<StripeWebhookEvent> StripeWebhookEvents => Set<StripeWebhookEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentIntegrationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
