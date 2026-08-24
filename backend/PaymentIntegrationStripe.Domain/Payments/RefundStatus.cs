namespace PaymentIntegrationStripe.Domain.Payments;

public enum RefundStatus
{
    Requested = 1,
    Processing = 2,
    Succeeded = 3,
    Failed = 4,
    Cancelled = 5
}
