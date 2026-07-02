using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure;

public class ReceiptCleaner(
    IServiceScopeFactory scopeFactory,
    ILogger<ReceiptCleaner> logger) : BackgroundService
{
    public TimeSpan Interval = TimeSpan.FromHours(24);
    public TimeSpan MinAge = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await CleanupAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Orphan receipt cleanup run failed.");
            }

            try
            {
                await Task.Delay(Interval, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task CleanupAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var storage = scope.ServiceProvider.GetRequiredService<ReceiptStorage>();

        var referenced = await db.Transactions
            .Where(t => t.ReceiptKey != null)
            .Select(t => t.ReceiptKey!)
            .ToHashSetAsync(ct);

        var objects = await storage.ListAsync(ct);
        var cutoff = DateTime.UtcNow - MinAge;

        var deleted = 0;
        foreach (var (key, lastModified) in objects)
        {
            if (referenced.Contains(key))
            {
                continue;
            }
            if (lastModified > cutoff)
            {
                continue;
            }

            try
            {
                await storage.DeleteAsync(key, ct);
                deleted++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to delete receipt {Key}", key);
            }
        }

        if (deleted > 0)
        {
            logger.LogInformation("Receipt cleanup removed {Count} file(s)", deleted);
        }
    }
}
