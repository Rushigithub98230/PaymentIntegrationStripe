using Microsoft.EntityFrameworkCore;
using PaymentIntegrationStripe.Domain.Catalog;
using PaymentIntegrationStripe.Domain.Identity;
using PaymentIntegrationStripe.Domain.Orders;
using PaymentIntegrationStripe.Domain.Payments;

namespace PaymentIntegrationStripe.Infrastructure.Persistence;

public sealed class PaymentIntegrationDbContext(DbContextOptions<PaymentIntegrationDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Inventory> Inventory => Set<Inventory>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
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
