using PaymentIntegrationStripe.Domain.Common;

namespace PaymentIntegrationStripe.Domain.Payments;

public sealed class PaymentAttempt : Entity
{
    private PaymentAttempt() { }
    public PaymentAttempt(Guid paymentId, int attemptNumber, string idempotencyKey)
    {
        if (paymentId == Guid.Empty) throw new ArgumentException("Payment ID is required.", nameof(paymentId));
        if (attemptNumber <= 0) throw new ArgumentOutOfRangeException(nameof(attemptNumber));
        if (string.IsNullOrWhiteSpace(idempotencyKey)) throw new ArgumentException("Idempotency key is required.", nameof(idempotencyKey));
        PaymentId = paymentId; AttemptNumber = attemptNumber; IdempotencyKey = idempotencyKey.Trim();
    }
    public Guid PaymentId { get; private set; }
    public int AttemptNumber { get; private set; }
    public string IdempotencyKey { get; private set; } = null!;
    public string? ProviderPaymentIntentId { get; private set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Created;
    public string? FailureCode { get; private set; }
    public string? FailureMessage { get; private set; }
    public void SetProviderPaymentIntent(string id) => ProviderPaymentIntentId = id;
    public void TransitionTo(PaymentStatus status, string? code = null, string? message = null)
    {
        if (!PaymentStateMachine.CanTransition(Status, status)) throw new InvalidOperationException($"Invalid payment attempt transition: {Status} -> {status}.");
        Status = status; FailureCode = code; FailureMessage = message; UpdatedAtUtc = DateTime.UtcNow;
    }
}
