using PaymentIntegrationStripe.Domain.Common;

namespace PaymentIntegrationStripe.Domain.Payments;

public sealed class StripeWebhookEvent : Entity
{
    private StripeWebhookEvent() { }

    public StripeWebhookEvent(string stripeEventId, string eventType, string payloadHash, DateTime receivedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(stripeEventId)) throw new ArgumentException("Stripe event ID is required.", nameof(stripeEventId));
        if (string.IsNullOrWhiteSpace(eventType)) throw new ArgumentException("Stripe event type is required.", nameof(eventType));
        if (string.IsNullOrWhiteSpace(payloadHash)) throw new ArgumentException("Webhook payload hash is required.", nameof(payloadHash));

        StripeEventId = stripeEventId.Trim();
        EventType = eventType.Trim();
        PayloadHash = payloadHash.Trim();
        ReceivedAtUtc = receivedAtUtc;
    }

    public string StripeEventId { get; private set; } = null!;
    public string EventType { get; private set; } = null!;
    public string PayloadHash { get; private set; } = null!;
    public DateTime ReceivedAtUtc { get; private set; }
    public bool Processed { get; private set; }
    public DateTime? ProcessedAtUtc { get; private set; }
    public int AttemptCount { get; private set; }
    public string? LastError { get; private set; }

    public void MarkProcessed()
    {
        Processed = true;
        ProcessedAtUtc = DateTime.UtcNow;
        LastError = null;
        AttemptCount++;
    }

    public void RecordFailure(string error)
    {
        if (string.IsNullOrWhiteSpace(error)) throw new ArgumentException("Webhook failure error is required.", nameof(error));
        AttemptCount++;
        LastError = error.Trim();
    }
}
