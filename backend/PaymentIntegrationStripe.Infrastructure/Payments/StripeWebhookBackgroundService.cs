using Microsoft.EntityFrameworkCore;
using PaymentIntegrationStripe.Application.Payments;
using PaymentIntegrationStripe.Infrastructure.Persistence;

namespace PaymentIntegrationStripe.Infrastructure.Payments;

public sealed class StripeWebhookBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<StripeWebhookBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Stripe webhook worker batch failed.");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PaymentIntegrationDbContext>();
        var processor = scope.ServiceProvider.GetRequiredService<IStripeWebhookProcessor>();

        var events = await db.StripeWebhookEvents
            .Where(x => !x.Processed && x.AttemptCount < 10)
            .OrderBy(x => x.ReceivedAtUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var webhookEvent in events)
        {
            try
            {
                await processor.ProcessAsync(
                    webhookEvent.StripeEventId,
                    webhookEvent.EventType,
                    webhookEvent.Payload,
                    cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Stripe webhook {EventId} processing failed.", webhookEvent.StripeEventId);
            }
        }
    }
}
