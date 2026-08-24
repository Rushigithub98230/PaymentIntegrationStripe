using PaymentIntegrationStripe.Domain.Payments;

namespace PaymentIntegrationStripe.Tests;

public class RefundStateMachineTests
{
    [Fact]
    public void Requested_to_Processing_is_valid()
    {
        Assert.True(RefundStateMachine.CanTransition(RefundStatus.Requested, RefundStatus.Processing));
    }

    [Fact]
    public void Processing_to_Succeeded_is_valid()
    {
        Assert.True(RefundStateMachine.CanTransition(RefundStatus.Processing, RefundStatus.Succeeded));
    }

    [Fact]
    public void Succeeded_to_Processing_is_invalid()
    {
        Assert.False(RefundStateMachine.CanTransition(RefundStatus.Succeeded, RefundStatus.Processing));
    }

    [Fact]
    public void Failed_to_Processing_is_valid_for_recovery()
    {
        Assert.True(RefundStateMachine.CanTransition(RefundStatus.Failed, RefundStatus.Processing));
    }
}
