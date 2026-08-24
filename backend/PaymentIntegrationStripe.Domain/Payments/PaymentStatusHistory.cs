using PaymentIntegrationStripe.Domain.Common;

namespace PaymentIntegrationStripe.Domain.Payments;

public sealed class PaymentStatusHistory : Entity
{
    private PaymentStatusHistory() { }
    public PaymentStatusHistory(Guid paymentId, PaymentStatus previousStatus, PaymentStatus newStatus, string source)
    {
        PaymentId = paymentId; PreviousStatus = previousStatus; NewStatus = newStatus; Source = source.Trim();
    }
    public Guid PaymentId { get; private set; }
    public PaymentStatus PreviousStatus { get; private set; }
    public PaymentStatus NewStatus { get; private set; }
    public string Source { get; private set; } = null!;
}
