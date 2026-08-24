namespace PaymentIntegrationStripe.Domain.Orders;

public static class OrderStateMachine
{
    public static bool CanTransition(OrderStatus current, OrderStatus next)
    {
        if (current == next) return true;

        return current switch
        {
            OrderStatus.PendingPayment => next is OrderStatus.Paid or OrderStatus.Cancelled,
            OrderStatus.Paid => next is OrderStatus.Processing or OrderStatus.Cancelled or OrderStatus.PartiallyRefunded or OrderStatus.Refunded,
            OrderStatus.Processing => next is OrderStatus.Packed or OrderStatus.Cancelled or OrderStatus.PartiallyRefunded or OrderStatus.Refunded,
            OrderStatus.Packed => next is OrderStatus.Shipped or OrderStatus.Cancelled or OrderStatus.PartiallyRefunded or OrderStatus.Refunded,
            OrderStatus.Shipped => next is OrderStatus.Delivered or OrderStatus.Returned or OrderStatus.PartiallyRefunded or OrderStatus.Refunded,
            OrderStatus.Delivered => next is OrderStatus.Returned or OrderStatus.PartiallyRefunded or OrderStatus.Refunded,
            OrderStatus.Returned => next is OrderStatus.PartiallyRefunded or OrderStatus.Refunded,
            OrderStatus.PartiallyRefunded => next is OrderStatus.Refunded,
            OrderStatus.Cancelled => false,
            OrderStatus.Refunded => false,
            _ => false
        };
    }
}
