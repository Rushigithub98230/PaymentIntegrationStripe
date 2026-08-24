using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PaymentIntegrationStripe.Domain.Payments;
using PaymentIntegrationStripe.Infrastructure.Persistence;
using PaymentIntegrationStripe.Infrastructure.Payments.Stripe;
using Stripe;

namespace PaymentIntegrationStripe.Api.Controllers;

[ApiController]
[Route("api/payments/stripe/webhook")]
public sealed class StripeWebhookController(
    PaymentIntegrationDbContext db,
    IOptions<StripeOptions> stripeOptions,
    ILogger<StripeWebhookController> logger) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Handle(CancellationToken cancellationToken)
    {
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        Request.Body.Position = 0;

        if (string.IsNullOrWhiteSpace(stripeOptions.Value.WebhookSecret))
            return StatusCode(StatusCodes.Status500InternalServerError, "Stripe webhook secret is not configured.");

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                payload,
                Request.Headers.StripeSignature.ToString(),
                stripeOptions.Value.WebhookSecret);
        }
        catch (StripeException ex)
        {
            logger.LogWarning(ex, "Invalid Stripe webhook signature.");
            return Unauthorized();
        }

        var payloadHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
        var existing = await db.StripeWebhookEvents
            .SingleOrDefaultAsync(x => x.StripeEventId == stripeEvent.Id, cancellationToken);

        if (existing is not null)
            return existing.Processed ? Ok() : Accepted();

        var webhookEvent = new StripeWebhookEvent(
            stripeEvent.Id,
            stripeEvent.Type,
            payloadHash,
            DateTime.UtcNow);

        await db.StripeWebhookEvents.AddAsync(webhookEvent, cancellationToken);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            var raced = await db.StripeWebhookEvents
                .SingleOrDefaultAsync(x => x.StripeEventId == stripeEvent.Id, cancellationToken);
            return raced is not null ? Ok() : StatusCode(StatusCodes.Status500InternalServerError);
        }

        // Event persistence is intentionally separated from payment mutation.
        // A worker can safely retry this persisted event without receiving Stripe again.
        webhookEvent.MarkProcessed();
        await db.SaveChangesAsync(cancellationToken);

        return Ok();
    }
}
