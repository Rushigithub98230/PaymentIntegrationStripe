using Microsoft.EntityFrameworkCore;
using PaymentIntegrationStripe.Application.Payments;
using PaymentIntegrationStripe.Domain.Payments;
using PaymentIntegrationStripe.Infrastructure.Persistence;

namespace PaymentIntegrationStripe.Infrastructure.Payments;

public sealed class EfPaymentRepository(PaymentIntegrationDbContext db) : IPaymentRepository
{
    public Task<Payment?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken) =>
        db.Payments
            .Include(x => x.Attempts)
            .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken) =>
        await db.Payments.AddAsync(payment, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);
}
