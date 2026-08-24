using PaymentIntegrationStripe.Domain.Payments;

namespace PaymentIntegrationStripe.Tests;

public class PaymentStateMachineTests
{
    [Fact]
    public void Pending_to_Succeeded_is_valid()
    {
        Assert.True(PaymentStateMachine.CanTransition(PaymentStatus.Pending, PaymentStatus.Succeeded));
    }

    [Fact]
    public void Failed_to_Succeeded_is_invalid()
    {
        Assert.False(PaymentStateMachine.CanTransition(PaymentStatus.Failed, PaymentStatus.Succeeded));
    }

    [Fact]
    public void Succeeded_to_Refunded_is_valid()
    {
        Assert.True(PaymentStateMachine.CanTransition(PaymentStatus.Succeeded, PaymentStatus.Refunded));
    }
}
