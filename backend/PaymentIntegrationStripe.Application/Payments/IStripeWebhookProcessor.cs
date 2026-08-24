namespace PaymentIntegrationStripe.Application.Payments;

public interface IStripeWebhookProcessor
{
    Task ProcessAsync(string eventId, string eventType, string payload, CancellationToken cancellationToken);
}
