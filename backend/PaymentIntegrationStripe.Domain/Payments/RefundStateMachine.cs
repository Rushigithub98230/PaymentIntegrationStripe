namespace PaymentIntegrationStripe.Domain.Payments;

public static class RefundStateMachine
{
    public static bool CanTransition(RefundStatus current, RefundStatus next)
    {
        if (current == next) return true;

        return current switch
        {
            RefundStatus.Requested => next is RefundStatus.Processing or RefundStatus.Cancelled,
            RefundStatus.Processing => next is RefundStatus.Succeeded or RefundStatus.Failed or RefundStatus.Cancelled,
            RefundStatus.Succeeded => false,
            RefundStatus.Failed => next is RefundStatus.Processing or RefundStatus.Cancelled,
            RefundStatus.Cancelled => false,
            _ => false
        };
    }
}
