using PaymentIntegrationStripe.Domain.Common;

namespace PaymentIntegrationStripe.Domain.Payments;

public sealed class PaymentTransaction : Entity
{
    private PaymentTransaction() { }
    public PaymentTransaction(Guid paymentId, string type, decimal amount, string currency)
    {
        if (paymentId == Guid.Empty) throw new ArgumentException("Payment ID is required.", nameof(paymentId));
        if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Transaction type is required.", nameof(type));
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        PaymentId = paymentId; Type = type.Trim(); Amount = decimal.Round(amount, 2); Currency = currency.Trim().ToLowerInvariant();
    }
    public Guid PaymentId { get; private set; }
    public string Type { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;
    public string? ProviderTransactionId { get; private set; }
    public void SetProviderTransaction(string id) => ProviderTransactionId = id;
}
