using PaymentIntegrationStripe.Domain.Payments;

namespace PaymentIntegrationStripe.Application.Payments;

public sealed class PaymentService(IPaymentGateway paymentGateway, IPaymentRepository paymentRepository)
{
    public async Task<PaymentCreationResult> CreatePaymentIntentAsync(CreatePaymentCommand command, CancellationToken cancellationToken)
    {
        Validate(command);
        var currency = command.Currency.Trim().ToLowerInvariant();
        var idempotencyKey = command.IdempotencyKey.Trim();
        var requestedAmount = decimal.Round(command.Amount, 2, MidpointRounding.ToEven);

        var existing = await paymentRepository.GetByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
        if (existing is not null)
            return ValidateAndReturnExisting(existing, command.OrderId, requestedAmount, currency);

        var payment = new Payment(command.OrderId, requestedAmount, currency, idempotencyKey);
        if (!string.IsNullOrWhiteSpace(command.ProviderCustomerId))
            payment.SetProviderCustomer(command.ProviderCustomerId);

        await paymentRepository.AddAsync(payment, cancellationToken);
        await paymentRepository.AddAttemptAsync(new PaymentAttempt(payment.Id, 1, idempotencyKey), cancellationToken);

        // Reserve the application idempotency key before calling Stripe. The database unique
        // constraint prevents concurrent requests from both reaching Stripe for the same key.
        try
        {
            await paymentRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (IsUniqueConstraintViolation(ex))
        {
            var concurrentPayment = await paymentRepository.GetByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
            if (concurrentPayment is null) throw;
            return ValidateAndReturnExisting(concurrentPayment, command.OrderId, requestedAmount, currency);
        }

        var gatewayResult = await paymentGateway.CreatePaymentIntentAsync(
            new CreatePaymentIntentRequest(
                currency,
                ToMinorUnits(requestedAmount, currency),
                idempotencyKey,
                command.ProviderCustomerId,
                command.OrderId.ToString("N"),
                new Dictionary<string, string>
                {
                    ["order_id"] = command.OrderId.ToString(),
                    ["payment_id"] = payment.Id.ToString(),
                    ["idempotency_key"] = idempotencyKey
                }),
            cancellationToken);

        payment.SetProviderPaymentIntent(gatewayResult.ProviderPaymentIntentId);
        if (!string.IsNullOrWhiteSpace(gatewayResult.ProviderChargeId))
            payment.SetProviderCharge(gatewayResult.ProviderChargeId);
        payment.TransitionTo(MapStatus(gatewayResult.ProviderStatus), gatewayResult.FailureCode, gatewayResult.FailureMessage);
        await paymentRepository.SaveChangesAsync(cancellationToken);

        return ToResult(payment, gatewayResult.FailureCode, gatewayResult.FailureMessage);
    }

    private static PaymentCreationResult ValidateAndReturnExisting(Payment existing, Guid orderId, decimal amount, string currency)
    {
        if (existing.OrderId != orderId || existing.Amount != amount || !string.Equals(existing.Currency, currency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The idempotency key is already associated with a different payment request.");
        return ToResult(existing);
    }

    private static PaymentStatus MapStatus(string providerStatus) => providerStatus switch
    {
        "succeeded" => PaymentStatus.Succeeded,
        "processing" => PaymentStatus.Processing,
        "requires_action" => PaymentStatus.RequiresAction,
        "requires_payment_method" => PaymentStatus.RequiresPaymentMethod,
        "canceled" => PaymentStatus.Cancelled,
        _ => PaymentStatus.Pending
    };

    private static PaymentCreationResult ToResult(Payment payment, string? failureCode = null, string? failureMessage = null) =>
        new(payment.ProviderPaymentIntentId ?? string.Empty, payment.Status.ToString(), payment.Status == PaymentStatus.Succeeded,
            failureCode ?? payment.LastFailureCode, failureMessage ?? payment.LastFailureMessage);

    private static void Validate(CreatePaymentCommand command)
    {
        if (command.OrderId == Guid.Empty) throw new ArgumentException("Order ID is required.", nameof(command));
        if (command.Amount <= 0) throw new ArgumentOutOfRangeException(nameof(command.Amount));
        if (string.IsNullOrWhiteSpace(command.Currency)) throw new ArgumentException("Currency is required.", nameof(command));
        if (string.IsNullOrWhiteSpace(command.IdempotencyKey)) throw new ArgumentException("Idempotency key is required.", nameof(command));
    }

    private static long ToMinorUnits(decimal amount, string currency) =>
        checked((long)decimal.Round(amount * (currency is "jpy" or "krw" ? 1m : 100m), 0, MidpointRounding.ToEven));

    private static bool IsUniqueConstraintViolation(Exception exception) =>
        exception.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
        exception.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) ||
        (exception.InnerException is not null && IsUniqueConstraintViolation(exception.InnerException));
}

public sealed record CreatePaymentCommand(Guid OrderId, decimal Amount, string Currency, string IdempotencyKey, string? ProviderCustomerId);
public sealed record PaymentCreationResult(string ProviderPaymentIntentId, string ProviderStatus, bool Succeeded, string? FailureCode, string? FailureMessage);
