using PaymentIntegrationStripe.Domain.Payments;
using Xunit;

namespace PaymentIntegrationStripe.Application.Payments;

public sealed class PaymentServiceTests
{
    [Fact]
    public async Task CreatePaymentIntent_ConvertsStandardCurrencyToMinorUnits()
    {
        var gateway = new FakePaymentGateway();
        var service = new PaymentService(gateway);

        await service.CreatePaymentIntentAsync(
            new CreatePaymentCommand(Guid.NewGuid(), 125.50m, "USD", "idem-1", "cus_123"),
            CancellationToken.None);

        Assert.Equal(12550, gateway.LastRequest!.AmountMinor);
        Assert.Equal("usd", gateway.LastRequest.Currency);
    }

    [Fact]
    public async Task CreatePaymentIntent_DoesNotMultiplyZeroDecimalCurrencyBy100()
    {
        var gateway = new FakePaymentGateway();
        var service = new PaymentService(gateway);

        await service.CreatePaymentIntentAsync(
            new CreatePaymentCommand(Guid.NewGuid(), 125m, "JPY", "idem-2", "cus_123"),
            CancellationToken.None);

        Assert.Equal(125, gateway.LastRequest!.AmountMinor);
    }

    [Fact]
    public async Task CreatePaymentIntent_ForwardsApplicationIdempotencyKey()
    {
        var gateway = new FakePaymentGateway();
        var service = new PaymentService(gateway);

        await service.CreatePaymentIntentAsync(
            new CreatePaymentCommand(Guid.NewGuid(), 10m, " idem-3 ", " usd ", "cus_123"),
            CancellationToken.None);

        Assert.Equal("idem-3", gateway.LastRequest!.IdempotencyKey);
    }

    private sealed class FakePaymentGateway : IPaymentGateway
    {
        public CreatePaymentIntentRequest? LastRequest { get; private set; }

        public Task<PaymentGatewayResult> CreatePaymentIntentAsync(CreatePaymentIntentRequest request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new PaymentGatewayResult(true, "pi_test", "succeeded", "ch_test", null, null));
        }

        public Task<PaymentGatewayResult> GetPaymentIntentAsync(string providerPaymentIntentId, CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task<RefundGatewayResult> CreateRefundAsync(CreateRefundRequest request, CancellationToken cancellationToken) =>
            throw new NotImplementedException();
    }
}
