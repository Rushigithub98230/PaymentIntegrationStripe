namespace PaymentIntegrationStripe.Domain.Orders;

public enum OrderStatus
{
    PendingPayment = 1,
    Paid = 2,
    Processing = 3,
    Packed = 4,
    Shipped = 5,
    Delivered = 6,
    Cancelled = 7,
    Returned = 8,
    PartiallyRefunded = 9,
    Refunded = 10
}
