using Stripe;

namespace PaymentIntegrationStripe.Application.Payments;

public interface IStripeWebhookSignatureVerifier
{
    Stripe.Event Verify(string payload, string signatureHeader, string webhookSecret);
}

public sealed class StripeWebhookSignatureVerifier : IStripeWebhookSignatureVerifier
{
    public Stripe.Event Verify(string payload, string signatureHeader, string webhookSecret)
    {
        if (string.IsNullOrWhiteSpace(payload)) throw new ArgumentException("Webhook payload is required.", nameof(payload));
        if (string.IsNullOrWhiteSpace(signatureHeader)) throw new ArgumentException("Stripe signature is required.", nameof(signatureHeader));
        if (string.IsNullOrWhiteSpace(webhookSecret)) throw new ArgumentException("Stripe webhook secret is required.", nameof(webhookSecret));

        return EventUtility.ConstructEvent(payload, signatureHeader, webhookSecret);
    }
}
