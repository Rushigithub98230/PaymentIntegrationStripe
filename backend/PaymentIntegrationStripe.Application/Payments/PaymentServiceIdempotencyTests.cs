using PaymentIntegrationStripe.Domain.Payments;
using Xunit;

namespace PaymentIntegrationStripe.Application.Payments;

public sealed class PaymentServiceIdempotencyTests
{
    [Fact]
    public async Task SameKeyAndSameRequest_ReturnsExistingPaymentWithoutCallingGateway()
    {
        var existing = new Payment(Guid.NewGuid(), 100m, "usd", "idem-1");
        var repository = new FakePaymentRepository(existing);
        var gateway = new FakePaymentGateway();
        var service = new PaymentService(gateway, repository);

        var result = await service.CreatePaymentIntentAsync(
            new CreatePaymentCommand(existing.OrderId, 100m, "USD", "idem-1", "cus_1"), CancellationToken.None);

        Assert.Equal(0, gateway.CreateCalls);
        Assert.Equal(existing.Id, repository.LastReturned!.Id);
        Assert.Equal(string.Empty, result.ProviderPaymentIntentId);
    }

    [Fact]
    public async Task SameKeyWithDifferentAmount_IsRejected()
    {
        var existing = new Payment(Guid.NewGuid(), 100m, "usd", "idem-2");
        var service = new PaymentService(new FakePaymentGateway(), new FakePaymentRepository(existing));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreatePaymentIntentAsync(
            new CreatePaymentCommand(existing.OrderId, 101m, "USD", "idem-2", "cus_1"), CancellationToken.None));
    }

    private sealed class FakePaymentRepository(Payment existing) : IPaymentRepository
    {
        public Payment? LastReturned { get; private set; }
        public Task<Payment?> GetByIdempotencyKeyAsync(string key, CancellationToken ct)
        { LastReturned = existing; return Task.FromResult<Payment?>(existing); }
        public Task AddAsync(Payment payment, CancellationToken ct) => Task.CompletedTask;
        public Task AddAttemptAsync(PaymentAttempt attempt, CancellationToken ct) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
    }

    private sealed class FakePaymentGateway : IPaymentGateway
    {
        public int CreateCalls { get; private set; }
        public Task<PaymentGatewayResult> CreatePaymentIntentAsync(CreatePaymentIntentRequest request, CancellationToken ct)
        { CreateCalls++; return Task.FromResult(new PaymentGatewayResult(true, "pi_test", "succeeded", "ch_test", null, null)); }
        public Task<PaymentGatewayResult> GetPaymentIntentAsync(string id, CancellationToken ct) => throw new NotImplementedException();
        public Task<RefundGatewayResult> CreateRefundAsync(CreateRefundRequest request, CancellationToken ct) => throw new NotImplementedException();
    }
}
