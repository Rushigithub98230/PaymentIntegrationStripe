using PaymentIntegrationStripe.Domain.Common;

namespace PaymentIntegrationStripe.Domain.Payments;

public sealed class Payment : Entity
{
    private Payment() { }

    public Payment(Guid orderId, decimal amount, string currency, string idempotencyKey)
    {
        if (orderId == Guid.Empty) throw new ArgumentException("Order ID is required.", nameof(orderId));
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency is required.", nameof(currency));
        if (string.IsNullOrWhiteSpace(idempotencyKey)) throw new ArgumentException("Idempotency key is required.", nameof(idempotencyKey));

        OrderId = orderId;
        Amount = decimal.Round(amount, 2, MidpointRounding.ToEven);
        Currency = currency.Trim().ToLowerInvariant();
        IdempotencyKey = idempotencyKey.Trim();
        Status = PaymentStatus.Created;
    }

    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;
    public PaymentStatus Status { get; private set; }
    public string IdempotencyKey { get; private set; } = null!;
    public string? ProviderCustomerId { get; private set; }
    public string? ProviderPaymentIntentId { get; private set; }
    public string? ProviderChargeId { get; private set; }
    public decimal AmountRefunded { get; private set; }
    public string? LastFailureCode { get; private set; }
    public string? LastFailureMessage { get; private set; }

    public void SetProviderCustomer(string customerId) => ProviderCustomerId = customerId;

    public void SetProviderPaymentIntent(string paymentIntentId) => ProviderPaymentIntentId = paymentIntentId;

    public void SetProviderCharge(string chargeId) => ProviderChargeId = chargeId;

    public void TransitionTo(PaymentStatus next, string? failureCode = null, string? failureMessage = null)
    {
        if (!PaymentStateMachine.CanTransition(Status, next))
            throw new InvalidOperationException($"Invalid payment transition: {Status} -> {next}.");

        Status = next;
        LastFailureCode = failureCode;
        LastFailureMessage = failureMessage;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public decimal GetRemainingRefundableAmount() => Amount - AmountRefunded;

    public void ApplyRefund(decimal refundAmount)
    {
        if (refundAmount <= 0) throw new ArgumentOutOfRangeException(nameof(refundAmount));
        if (Status is not (PaymentStatus.Succeeded or PaymentStatus.PartiallyRefunded or PaymentStatus.RefundPending))
            throw new InvalidOperationException("Only successful payments can be refunded.");

        var remaining = GetRemainingRefundableAmount();
        if (refundAmount > remaining)
            throw new InvalidOperationException("Refund amount exceeds the remaining refundable amount.");

        AmountRefunded += decimal.Round(refundAmount, 2, MidpointRounding.ToEven);
        var target = AmountRefunded == Amount ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded;
        if (Status != target) TransitionTo(target);
    }
}
