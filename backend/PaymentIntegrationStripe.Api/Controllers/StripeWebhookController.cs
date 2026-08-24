using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
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

        var signature = Request.Headers.StripeSignature.ToString();
        if (string.IsNullOrWhiteSpace(signature))
            return Unauthorized();

        var secret = stripeOptions.Value.WebhookSecret;
        if (string.IsNullOrWhiteSpace(secret))
            return StatusCode(StatusCodes.Status500InternalServerError, "Stripe webhook secret is not configured.");

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(payload, signature, secret);
        }
        catch (StripeException ex)
        {
            logger.LogWarning(ex, "Invalid Stripe webhook signature.");
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(stripeEvent.Id))
            return BadRequest("Stripe event ID is missing.");

        var existing = await db.StripeWebhookEvents
            .SingleOrDefaultAsync(x => x.StripeEventId == stripeEvent.Id, cancellationToken);

        if (existing is not null)
        {
            // A previously accepted event is already durable. The worker can retry it
            // when Processed=false; no duplicate financial operation is created here.
            return existing.Processed ? Ok() : Accepted();
        }

        var payloadHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
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
        catch (DbUpdateException ex)
        {
            // A concurrent delivery may have inserted the same Stripe event ID.
            // Only treat it as a duplicate when the event is now present; unrelated
            // database failures must surface as a server error.
            logger.LogInformation(ex, "Concurrent Stripe webhook persistence detected for {StripeEventId}.", stripeEvent.Id);
            var raced = await db.StripeWebhookEvents
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.StripeEventId == stripeEvent.Id, cancellationToken);

            if (raced is not null)
                return raced.Processed ? Ok() : Accepted();

            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        // Persist first. A background worker processes this durable event transactionally.
        return Accepted();
    }
}
