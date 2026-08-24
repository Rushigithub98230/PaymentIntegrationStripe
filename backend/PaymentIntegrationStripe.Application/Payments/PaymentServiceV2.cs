namespace PaymentIntegrationStripe.Application.Payments;

// Transitional application orchestration contract. The existing PaymentService is kept unchanged
// until its current blob is fetched and updated safely against the repository's latest SHA.
public sealed record PaymentRequestFingerprint(Guid OrderId, decimal Amount, string Currency, string IdempotencyKey);
