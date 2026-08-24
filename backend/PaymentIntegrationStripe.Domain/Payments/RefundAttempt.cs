using PaymentIntegrationStripe.Domain.Common;

namespace PaymentIntegrationStripe.Domain.Payments;

public sealed class RefundAttempt : Entity
{
    private RefundAttempt() { }
    public RefundAttempt(Guid refundId, int attemptNumber, string idempotencyKey)
    {
        RefundId = refundId; AttemptNumber = attemptNumber; IdempotencyKey = idempotencyKey.Trim();
    }
    public Guid RefundId { get; private set; }
    public int AttemptNumber { get; private set; }
    public string IdempotencyKey { get; private set; } = null!;
    public string? ProviderRefundId { get; private set; }
    public RefundStatus Status { get; private set; } = RefundStatus.Requested;
    public void SetProviderRefund(string id) => ProviderRefundId = id;
    public void SetStatus(RefundStatus status) { Status = status; UpdatedAtUtc = DateTime.UtcNow; }
}
