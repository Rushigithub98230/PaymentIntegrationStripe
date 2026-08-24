using PaymentIntegrationStripe.Domain.Payments;

namespace PaymentIntegrationStripe.Tests;

public class PaymentFinancialInvariantTests
{
    [Fact]
    public void Payment_rejects_refund_larger_than_remaining_amount()
    {
        var payment = new Payment(Guid.NewGuid(), 5000m, "INR", "payment-key-1");
        payment.TransitionTo(PaymentStatus.Pending);
        payment.TransitionTo(PaymentStatus.Succeeded);

        payment.ApplyRefund(3000m);

        Assert.Equal(2000m, payment.GetRemainingRefundableAmount());
        Assert.Equal(PaymentStatus.PartiallyRefunded, payment.Status);
        Assert.Throws<InvalidOperationException>(() => payment.ApplyRefund(2001m));
    }

    [Fact]
    public void Exact_final_refund_moves_payment_to_refunded()
    {
        var payment = new Payment(Guid.NewGuid(), 5000m, "INR", "payment-key-2");
        payment.TransitionTo(PaymentStatus.Pending);
        payment.TransitionTo(PaymentStatus.Succeeded);

        payment.ApplyRefund(5000m);

        Assert.Equal(0m, payment.GetRemainingRefundableAmount());
        Assert.Equal(PaymentStatus.Refunded, payment.Status);
    }

    [Fact]
    public void Failed_payment_cannot_be_refunded()
    {
        var payment = new Payment(Guid.NewGuid(), 5000m, "INR", "payment-key-3");
        payment.TransitionTo(PaymentStatus.Pending);
        payment.TransitionTo(PaymentStatus.Failed, "card_declined", "Card declined");

        Assert.Throws<InvalidOperationException>(() => payment.ApplyRefund(100m));
    }
}
