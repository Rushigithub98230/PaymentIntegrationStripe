namespace PaymentIntegrationStripe.Domain.Payments;

public static class PaymentStateMachine
{
    public static bool CanTransition(PaymentStatus current, PaymentStatus next)
    {
        if (current == next) return true;

        return current switch
        {
            PaymentStatus.Created => next is PaymentStatus.Pending or PaymentStatus.Cancelled,
            PaymentStatus.Pending => next is PaymentStatus.RequiresAction or PaymentStatus.Processing or PaymentStatus.Authorized or PaymentStatus.Succeeded or PaymentStatus.Failed or PaymentStatus.Cancelled,
            PaymentStatus.RequiresAction => next is PaymentStatus.Processing or PaymentStatus.Authorized or PaymentStatus.Succeeded or PaymentStatus.Failed or PaymentStatus.Cancelled,
            PaymentStatus.Processing => next is PaymentStatus.Authorized or PaymentStatus.Succeeded or PaymentStatus.Failed or PaymentStatus.Cancelled,
            PaymentStatus.Authorized => next is PaymentStatus.Succeeded or PaymentStatus.Failed or PaymentStatus.Cancelled,
            PaymentStatus.Succeeded => next is PaymentStatus.RefundPending or PaymentStatus.PartiallyRefunded or PaymentStatus.Refunded,
            PaymentStatus.PartiallyRefunded => next is PaymentStatus.RefundPending or PaymentStatus.PartiallyRefunded or PaymentStatus.Refunded,
            PaymentStatus.RefundPending => next is PaymentStatus.PartiallyRefunded or PaymentStatus.Refunded or PaymentStatus.Succeeded,
            PaymentStatus.Failed => false,
            PaymentStatus.Cancelled => false,
            PaymentStatus.Refunded => false,
            _ => false
        };
    }
}
