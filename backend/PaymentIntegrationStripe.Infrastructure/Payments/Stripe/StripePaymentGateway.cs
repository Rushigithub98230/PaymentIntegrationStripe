using Microsoft.Extensions.Options;
using PaymentIntegrationStripe.Application.Payments;
using Stripe;

namespace PaymentIntegrationStripe.Infrastructure.Payments.Stripe;

public sealed class StripePaymentGateway(IOptions<StripeOptions> options) : IPaymentGateway
{
    private readonly StripeOptions _options = options.Value;

    public async Task<PaymentGatewayResult> CreatePaymentIntentAsync(
        CreatePaymentIntentRequest request,
        CancellationToken cancellationToken)
    {
        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(new PaymentIntentCreateOptions
        {
            Amount = request.AmountMinor,
            Currency = request.Currency.ToLowerInvariant(),
            Customer = request.CustomerReference,
            Metadata = request.Metadata.ToDictionary(x => x.Key, x => x.Value),
        }, new RequestOptions
        {
            IdempotencyKey = request.IdempotencyKey,
        }, cancellationToken);

        return new PaymentGatewayResult(
            Succeeded: intent.Status == "succeeded",
            ProviderPaymentIntentId: intent.Id,
            ProviderStatus: intent.Status,
            ProviderChargeId: intent.LatestChargeId,
            FailureCode: intent.LastPaymentError?.Code,
            FailureMessage: intent.LastPaymentError?.Message);
    }

    public async Task<PaymentGatewayResult> GetPaymentIntentAsync(
        string providerPaymentIntentId,
        CancellationToken cancellationToken)
    {
        var service = new PaymentIntentService();
        var intent = await service.GetAsync(providerPaymentIntentId, null, null, cancellationToken);

        return new PaymentGatewayResult(
            Succeeded: intent.Status == "succeeded",
            ProviderPaymentIntentId: intent.Id,
            ProviderStatus: intent.Status,
            ProviderChargeId: intent.LatestChargeId,
            FailureCode: intent.LastPaymentError?.Code,
            FailureMessage: intent.LastPaymentError?.Message);
    }

    public async Task<RefundGatewayResult> CreateRefundAsync(
        CreateRefundRequest request,
        CancellationToken cancellationToken)
    {
        var service = new RefundService();
        var refund = await service.CreateAsync(new RefundCreateOptions
        {
            PaymentIntent = request.ProviderPaymentIntentId,
            Amount = request.AmountMinor,
            Reason = request.Reason,
        }, new RequestOptions
        {
            IdempotencyKey = request.IdempotencyKey,
        }, cancellationToken);

        return new RefundGatewayResult(
            Succeeded: refund.Status == "succeeded",
            ProviderRefundId: refund.Id,
            ProviderStatus: refund.Status ?? "unknown",
            FailureCode: null,
            FailureMessage: null);
    }
}
