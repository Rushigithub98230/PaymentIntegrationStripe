using PaymentIntegrationStripe.Domain.Common;

namespace PaymentIntegrationStripe.Domain.Payments;

public sealed class Refund : Entity
{
    private Refund() { }

    public Refund(Guid paymentId, decimal amount, string currency, string reason)
    {
        if (paymentId == Guid.Empty) throw new ArgumentException("Payment ID is required.", nameof(paymentId));
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency is required.", nameof(currency));
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Refund reason is required.", nameof(reason));

        PaymentId = paymentId;
        Amount = decimal.Round(amount, 2, MidpointRounding.ToEven);
        Currency = currency.Trim().ToLowerInvariant();
        Reason = reason.Trim();
        Status = RefundStatus.Requested;
    }

    public Guid PaymentId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;
    public string Reason { get; private set; } = null!;
    public RefundStatus Status { get; private set; }
    public string? ProviderRefundId { get; private set; }
    public string? FailureCode { get; private set; }
    public string? FailureMessage { get; private set; }

    public void SetProviderRefund(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Provider refund ID is required.", nameof(id));
        ProviderRefundId = id.Trim();
    }

    public void TransitionTo(RefundStatus status, string? code = null, string? message = null)
    {
        if (!RefundStateMachine.CanTransition(Status, status))
            throw new InvalidOperationException($"Invalid refund transition: {Status} -> {status}.");

        Status = status;
        FailureCode = code;
        FailureMessage = message;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
