using PaymentIntegrationStripe.Domain.Common;

namespace PaymentIntegrationStripe.Domain.Payments;

public sealed class RefundAttempt : Entity
{
    private RefundAttempt() { }

    public RefundAttempt(Guid refundId, int attemptNumber, string idempotencyKey)
    {
        if (refundId == Guid.Empty) throw new ArgumentException("Refund ID is required.", nameof(refundId));
        if (attemptNumber <= 0) throw new ArgumentOutOfRangeException(nameof(attemptNumber));
        if (string.IsNullOrWhiteSpace(idempotencyKey)) throw new ArgumentException("Idempotency key is required.", nameof(idempotencyKey));

        RefundId = refundId;
        AttemptNumber = attemptNumber;
        IdempotencyKey = idempotencyKey.Trim();
    }

    public Guid RefundId { get; private set; }
    public int AttemptNumber { get; private set; }
    public string IdempotencyKey { get; private set; } = null!;
    public string? ProviderRefundId { get; private set; }
    public RefundStatus Status { get; private set; } = RefundStatus.Requested;

    public void SetProviderRefund(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Provider refund ID is required.", nameof(id));
        ProviderRefundId = id.Trim();
    }

    public void SetStatus(RefundStatus status)
    {
        Status = status;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
