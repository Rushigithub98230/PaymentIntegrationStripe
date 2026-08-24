using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PaymentIntegrationStripe.Application.Payments;
using PaymentIntegrationStripe.Domain.Payments;
using PaymentIntegrationStripe.Infrastructure.Persistence;

namespace PaymentIntegrationStripe.Infrastructure.Payments;

public sealed class StripeWebhookProcessor(PaymentIntegrationDbContext db) : IStripeWebhookProcessor
{
    public async Task ProcessAsync(string eventId, string eventType, string payload, CancellationToken cancellationToken)
    {
        var webhook = await db.StripeWebhookEvents.SingleOrDefaultAsync(x => x.EventId == eventId, cancellationToken)
            ?? throw new InvalidOperationException($"Stripe webhook event '{eventId}' was not persisted.");

        if (webhook.ProcessedAtUtc.HasValue)
            return;

        try
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;
            var objectElement = root.TryGetProperty("data", out var data) && data.TryGetProperty("object", out var obj)
                ? obj
                : default;

            switch (eventType)
            {
                case "payment_intent.succeeded":
                    await ProcessPaymentIntentAsync(objectElement, PaymentStatus.Succeeded, eventId, cancellationToken);
                    break;
                case "payment_intent.processing":
                    await ProcessPaymentIntentAsync(objectElement, PaymentStatus.Processing, eventId, cancellationToken);
                    break;
                case "payment_intent.payment_failed":
                    await ProcessPaymentIntentAsync(objectElement, PaymentStatus.Failed, eventId, cancellationToken);
                    break;
                case "payment_intent.canceled":
                    await ProcessPaymentIntentAsync(objectElement, PaymentStatus.Cancelled, eventId, cancellationToken);
                    break;
            }

            webhook.MarkProcessed(DateTime.UtcNow);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            webhook.MarkFailed(ex.Message);
            await db.SaveChangesAsync(cancellationToken);
            throw;
        }
    }

    private async Task ProcessPaymentIntentAsync(JsonElement intent, PaymentStatus targetStatus, string eventId, CancellationToken cancellationToken)
    {
        if (intent.ValueKind != JsonValueKind.Object || !intent.TryGetProperty("id", out var idProperty))
            throw new InvalidOperationException("Stripe PaymentIntent ID is missing from webhook payload.");

        var providerPaymentIntentId = idProperty.GetString();
        if (string.IsNullOrWhiteSpace(providerPaymentIntentId))
            throw new InvalidOperationException("Stripe PaymentIntent ID is empty.");

        var payment = await db.Payments.SingleOrDefaultAsync(
            x => x.ProviderPaymentIntentId == providerPaymentIntentId,
            cancellationToken);

        if (payment is null)
            throw new InvalidOperationException($"Payment for Stripe PaymentIntent '{providerPaymentIntentId}' was not found.");

        string? failureCode = null;
        string? failureMessage = null;
        if (intent.TryGetProperty("last_payment_error", out var error) && error.ValueKind == JsonValueKind.Object)
        {
            if (error.TryGetProperty("code", out var code)) failureCode = code.GetString();
            if (error.TryGetProperty("message", out var message)) failureMessage = message.GetString();
        }

        payment.ApplyProviderStatus(targetStatus, $"StripeWebhook:{eventId}");
        if (targetStatus == PaymentStatus.Failed && (!string.IsNullOrWhiteSpace(failureCode) || !string.IsNullOrWhiteSpace(failureMessage)))
            payment.RecordFailure(failureCode, failureMessage);
    }
}
