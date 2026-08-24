namespace PaymentIntegrationStripe.Domain.Payments;

public enum PaymentStatus
{
    Created = 1,
    Pending = 2,
    RequiresAction = 3,
    Processing = 4,
    Authorized = 5,
    Succeeded = 6,
    Failed = 7,
    Cancelled = 8,
    RefundPending = 9,
    PartiallyRefunded = 10,
    Refunded = 11
}
