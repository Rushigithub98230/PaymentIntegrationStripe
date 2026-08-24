using PaymentIntegrationStripe.Domain.Payments;

namespace PaymentIntegrationStripe.Application.Payments;

public interface IPaymentGateway
{
    Task<PaymentGatewayResult> CreatePaymentIntentAsync(CreatePaymentIntentRequest request, CancellationToken cancellationToken);
    Task<PaymentGatewayResult> GetPaymentIntentAsync(string providerPaymentIntentId, CancellationToken cancellationToken);
    Task<RefundGatewayResult> CreateRefundAsync(CreateRefundRequest request, CancellationToken cancellationToken);
}

public sealed record CreatePaymentIntentRequest(
    string Currency,
    long AmountMinor,
    string IdempotencyKey,
    string CustomerReference,
    string OrderReference,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record CreateRefundRequest(
    string ProviderPaymentIntentId,
    long AmountMinor,
    string Currency,
    string IdempotencyKey,
    string Reason);

public sealed record PaymentGatewayResult(
    bool Succeeded,
    string ProviderPaymentIntentId,
    string ProviderStatus,
    string? ProviderChargeId,
    string? FailureCode,
    string? FailureMessage);

public sealed record RefundGatewayResult(
    bool Succeeded,
    string ProviderRefundId,
    string ProviderStatus,
    string? FailureCode,
    string? FailureMessage);
