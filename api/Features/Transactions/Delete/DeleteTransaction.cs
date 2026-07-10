using Api.Infrastructure;

namespace Api.Features.Transactions.Delete;

public sealed record DeleteTransactionRequest;

public static class DeleteTransaction
{
    public static IEndpointRouteBuilder MapDeleteTransactionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/transactions/{id}", Handle)
            .WithTags("Transactions")
            .WithName("DeleteTransaction");

        return app;
    }

    private static async Task<IResult> Handle(
        Guid id,
        AppDbContext context,
        ReceiptStorage storage,
        ILogger<DeleteTransactionRequest> logger,
        CancellationToken ct)
    {
        var transaction = await context.Transactions.FindAsync([id], ct);

        if (transaction is null)
            return Results.NotFound("Transaction not found");

        var receiptKey = transaction.ReceiptKey;

        context.Transactions.Remove(transaction);
        await context.SaveChangesAsync(ct);

        if (receiptKey is not null)
        {
            try
            {
                await storage.DeleteAsync(receiptKey, ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Failed to delete receipt after deleting transaction {TransactionId}. Orphaned file left in storage.",
                    id);
            }
        }

        return Results.NoContent();
    }
}
