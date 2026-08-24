using PaymentIntegrationStripe.Domain.Payments;

namespace PaymentIntegrationStripe.Application.Payments;

public sealed class PaymentService(IPaymentGateway paymentGateway)
{
    public async Task<PaymentCreationResult> CreatePaymentIntentAsync(
        CreatePaymentCommand command,
        CancellationToken cancellationToken)
    {
        if (command.OrderId == Guid.Empty)
            throw new ArgumentException("Order ID is required.", nameof(command));
        if (command.Amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(command.Amount));
        if (string.IsNullOrWhiteSpace(command.Currency))
            throw new ArgumentException("Currency is required.", nameof(command));
        if (string.IsNullOrWhiteSpace(command.IdempotencyKey))
            throw new ArgumentException("Idempotency key is required.", nameof(command));

        var normalizedCurrency = command.Currency.Trim().ToLowerInvariant();
        var normalizedKey = command.IdempotencyKey.Trim();

        // Persistence/idempotency ownership belongs to the application repository layer.
        // This service intentionally does not infer an existing payment from Stripe.
        var gatewayResult = await paymentGateway.CreatePaymentIntentAsync(
            new CreatePaymentIntentRequest(
                normalizedCurrency,
                ToMinorUnits(command.Amount, normalizedCurrency),
                normalizedKey,
                command.ProviderCustomerId,
                command.OrderId.ToString("N"),
                new Dictionary<string, string>
                {
                    ["order_id"] = command.OrderId.ToString(),
                    ["idempotency_key"] = normalizedKey
                }),
            cancellationToken);

        return new PaymentCreationResult(
            gatewayResult.ProviderPaymentIntentId,
            gatewayResult.ProviderStatus,
            gatewayResult.Succeeded,
            gatewayResult.FailureCode,
            gatewayResult.FailureMessage);
    }

    private static long ToMinorUnits(decimal amount, string currency)
    {
        var zeroDecimal = currency is "jpy" or "krw";
        var multiplier = zeroDecimal ? 1m : 100m;
        var minor = decimal.Round(amount * multiplier, 0, MidpointRounding.ToEven);
        if (minor > long.MaxValue || minor < long.MinValue)
            throw new OverflowException("Payment amount is outside Stripe's supported integer range.");
        return checked((long)minor);
    }
}

public sealed record CreatePaymentCommand(
    Guid OrderId,
    decimal Amount,
    string Currency,
    string IdempotencyKey,
    string ProviderCustomerId);

public sealed record PaymentCreationResult(
    string ProviderPaymentIntentId,
    string ProviderStatus,
    bool Succeeded,
    string? FailureCode,
    string? FailureMessage);
